#!/usr/bin/env python3
"""Measure how close a capture of the rebuild is to a reference render of the same scene.

Usage:
    python tools/compare_view.py <reference> <capture> [options]

The tool measures, it does not explain. It reconciles the two geometries, reports the colour
statistics, a coarse 3x3 spatial breakdown, the offset at which the reference best matches the
capture and the correlation reached there, and writes a composite for the eye
(reference | aligned reference | capture | difference | both coarse luminance maps).

Both frames are brought to one canvas by scaling the reference to the capture's height and
centre-cropping both to the smaller width, so no pixel of the comparison is invented and the
report states the crop that was applied.

Exit codes: 0 measured, 1 below --min-zncc, 2 bad input.
"""

import argparse
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

GRID = 32
PANEL_HEIGHT = 448
LUMA_PANEL = 192
GAP = 10
TITLE = 22

FONT = None


def font():
    global FONT
    if FONT is None:
        try:
            FONT = ImageFont.load_default(size=13)
        except TypeError:
            FONT = ImageFont.load_default()
    return FONT


def load(path):
    image = Image.open(path)
    image.load()
    return image.convert("RGB")


def luma(rgb):
    return 0.2126 * rgb[..., 0] + 0.7152 * rgb[..., 1] + 0.0722 * rgb[..., 2]


def center_crop(img, width):
    w, h = img.size
    if w <= width:
        return img
    x = (w - width) // 2
    return img.crop((x, 0, x + width, h))


def scaled(img, height):
    w, h = img.size
    return img.resize((max(1, round(w * height / h)), height), Image.LANCZOS)


def coarse_grid(img, size=GRID):
    return np.asarray(img.resize((size, size), Image.BOX), dtype=np.float64)


def zncc(a, b):
    a = a - a.mean()
    b = b - b.mean()
    d = np.sqrt(float((a * a).sum()) * float((b * b).sum()))
    return 0.0 if d == 0.0 else float((a * b).sum() / d)


def best_shift(ref_map, cap_map, radius=4):
    """The coarse shift of the reference that best matches the capture, and the correlation there.

    A shift of (dx, dy) means the reference content has to move right by dx and down by dy cells
    to line up with the capture. The wrapped strips are excluded from every correlation.
    """
    n = ref_map.shape[0]
    best = (-2.0, 0, 0)
    for dy in range(-radius, radius + 1):
        for dx in range(-radius, radius + 1):
            shifted = np.roll(ref_map, (dy, dx), axis=(0, 1))
            m = radius
            a = shifted[m:n - m, m:n - m]
            b = cap_map[m:n - m, m:n - m]
            if a.size < n * n // 2:
                continue
            value = zncc(a, b)
            if value > best[0]:
                best = (value, dx, dy)
    return best


def shift_by(img, dx, dy):
    """Move the image content right by dx and down by dy pixels."""
    return img.transform(img.size, Image.AFFINE, (1, 0, -dx, 0, 1, -dy), resample=Image.BICUBIC)


def quadrants(rgb):
    h, w, _ = rgb.shape
    rows = []
    for ry in range(3):
        row = []
        for rx in range(3):
            block = rgb[h * ry // 3:h * (ry + 1) // 3, w * rx // 3:w * (rx + 1) // 3]
            row.append(block.reshape(-1, 3).mean(axis=0))
        rows.append(row)
    return rows


def top_colors(rgb, count=6):
    quantised = (np.asarray(rgb) // 16).astype(np.uint8).reshape(-1, 3)
    keys = (quantised[:, 0].astype(np.int32) * 256
            + quantised[:, 1].astype(np.int32) * 16
            + quantised[:, 2].astype(np.int32))
    values, counts = np.unique(keys, return_counts=True)
    order = np.argsort(counts)[::-1][:count]
    total = quantised.shape[0]
    out = []
    for i in order:
        key = int(values[i])
        colour = (((key // 256) * 16 + 8), (((key // 16) % 16) * 16 + 8), ((key % 16) * 16 + 8))
        out.append((colour, float(counts[i]) / total))
    return out


def ramp(values):
    """A black -> red -> yellow ramp for the 0..255 difference map."""
    t = np.clip(values / 255.0, 0.0, 1.0)
    stops = [(0.0, (0, 0, 0)), (0.5, (176, 24, 32)), (1.0, (255, 232, 64))]
    out = np.zeros(values.shape + (3,), dtype=np.uint8)
    for i in range(len(stops) - 1):
        lo, lo_col = stops[i]
        hi, hi_col = stops[i + 1]
        mask = (t >= lo) & (t <= hi)
        span = hi - lo
        for ch in range(3):
            out[..., ch][mask] = (lo_col[ch] + (t[mask] - lo) / span * (hi_col[ch] - lo_col[ch]))
    return out


def titled(img, text):
    canvas = Image.new("RGB", (img.width, img.height + TITLE), (18, 18, 22))
    canvas.paste(img, (0, TITLE))
    ImageDraw.Draw(canvas).text((6, 4), text, fill=(232, 232, 232), font=font())
    return canvas


def grey_map(coarse):
    values = np.clip(luma(coarse), 0, 255).astype(np.uint8)
    return Image.fromarray(np.stack([values] * 3, axis=-1), "RGB")


def compose(reference, aligned, capture, diff, ref_coarse, cap_coarse, path):
    panels = [
        titled(scaled(reference, PANEL_HEIGHT), "reference, as compared"),
        titled(scaled(aligned, PANEL_HEIGHT), "reference aligned to the capture"),
        titled(scaled(capture, PANEL_HEIGHT), "capture, the rebuild"),
        titled(Image.fromarray(ramp(diff).astype(np.uint8), "RGB")
               .resize((LUMA_PANEL, LUMA_PANEL), Image.NEAREST), "difference after alignment"),
        titled(grey_map(ref_coarse).resize((LUMA_PANEL, LUMA_PANEL), Image.NEAREST),
               "reference luma {0}x{0}".format(GRID)),
        titled(grey_map(cap_coarse).resize((LUMA_PANEL, LUMA_PANEL), Image.NEAREST),
               "capture luma {0}x{0}".format(GRID)),
    ]
    width = sum(p.width for p in panels) + GAP * (len(panels) - 1)
    height = max(p.height for p in panels)
    canvas = Image.new("RGB", (width, height), (18, 18, 22))
    x = 0
    for panel in panels:
        canvas.paste(panel, (x, 0))
        x += panel.width + GAP
    canvas.save(path)
    return path


def main(argv):
    parser = argparse.ArgumentParser(description="Compare a capture with a reference render.")
    parser.add_argument("reference")
    parser.add_argument("capture")
    parser.add_argument("--out", default=None, help="output directory, default the capture's own")
    parser.add_argument("--name", default="compare.png", help="composite file name")
    parser.add_argument("--crop", default=None,
                        help="crop the reference before scaling, as x,y,w,h in its own pixels")
    parser.add_argument("--min-zncc", type=float, default=None,
                        help="exit 1 when the best correlation is below this value")
    args = parser.parse_args(argv)

    for path in (args.reference, args.capture):
        if not os.path.isfile(path):
            print("missing input: " + path, file=sys.stderr)
            return 2

    reference = load(args.reference)
    if args.crop:
        parts = [int(v) for v in args.crop.split(",")]
        if len(parts) != 4:
            print("--crop needs x,y,w,h", file=sys.stderr)
            return 2
        x, y, w, h = parts
        if x < 0 or y < 0 or w <= 0 or h <= 0 or x + w > reference.width or y + h > reference.height:
            print("--crop is outside the reference image", file=sys.stderr)
            return 2
        reference = reference.crop((x, y, x + w, y + h))
    capture = load(args.capture)

    ref_size = reference.size
    cap_size = capture.size
    reference = scaled(reference, cap_size[1])
    width = min(reference.width, capture.width)
    reference = center_crop(reference, width)
    capture = center_crop(capture, width)

    ref_rgb = np.asarray(reference, dtype=np.float64)
    cap_rgb = np.asarray(capture, dtype=np.float64)
    ref_coarse = coarse_grid(reference)
    cap_coarse = coarse_grid(capture)

    zero = zncc(luma(ref_coarse), luma(cap_coarse))
    best, dx, dy = best_shift(luma(ref_coarse), luma(cap_coarse))
    px = dx * width / GRID
    py = dy * cap_size[1] / GRID
    aligned = shift_by(reference, px, py)
    aligned_rgb = np.asarray(aligned, dtype=np.float64)
    diff = np.abs(luma(coarse_grid(aligned)) - luma(cap_coarse))

    lines = []
    lines.append("inputs")
    lines.append("  reference : {0}  {1}x{2}  aspect {3:.4f}".format(
        args.reference, ref_size[0], ref_size[1], ref_size[0] / ref_size[1]))
    lines.append("  capture   : {0}  {1}x{2}  aspect {3:.4f}".format(
        args.capture, cap_size[0], cap_size[1], cap_size[0] / cap_size[1]))
    lines.append("  compared  : {0}x{1}  (the reference scaled to the capture's height, then both "
                 "centre-cropped to {0} px)".format(width, cap_size[1]))
    if args.crop:
        lines.append("  --crop    : " + args.crop)

    lines.append("")
    lines.append("frame statistics")
    lines.append("  {0:<14}{1:>20}{2:>20}".format("", "reference", "capture"))
    for label, ref_value, cap_value in (
            ("mean RGB",
             "[{0:.0f},{1:.0f},{2:.0f}]".format(*ref_rgb.reshape(-1, 3).mean(axis=0)),
             "[{0:.0f},{1:.0f},{2:.0f}]".format(*cap_rgb.reshape(-1, 3).mean(axis=0))),
            ("mean level", "{0:.1f}".format(float(luma(ref_rgb).mean())),
             "{0:.1f}".format(float(luma(cap_rgb).mean()))),
            ("near black", "{0:.1f}%".format(float((luma(ref_rgb) < 10).mean()) * 100),
             "{0:.1f}%".format(float((luma(cap_rgb) < 10).mean()) * 100)),
            ("bright >128", "{0:.1f}%".format(float((luma(ref_rgb) > 128).mean()) * 100),
             "{0:.1f}%".format(float((luma(cap_rgb) > 128).mean()) * 100))):
        lines.append("  {0:<14}{1:>20}{2:>20}".format(label, ref_value, cap_value))

    lines.append("")
    lines.append("mean RGB of the nine zones, left to right, top to bottom")
    for label, rgb in (("reference", ref_rgb), ("capture", cap_rgb)):
        lines.append("  " + label)
        for row in quadrants(rgb):
            lines.append("    " + "  ".join(
                "[{0:>3.0f},{1:>3.0f},{2:>3.0f}]".format(*cell) for cell in row))

    lines.append("")
    lines.append("dominant colours, 16 levels per channel")
    for label, rgb in (("reference", ref_rgb), ("capture", cap_rgb)):
        lines.append("  " + label)
        for colour, share in top_colors(rgb):
            lines.append("    [{0:>3},{1:>3},{2:>3}]  {3:5.1f}%".format(
                colour[0], colour[1], colour[2], share * 100))

    lines.append("")
    lines.append("alignment on the coarse {0}x{0} luminance maps".format(GRID))
    lines.append("  correlation at zero offset : {0:+.3f}".format(zero))
    lines.append("  best correlation           : {0:+.3f} at {1:+d},{2:+d} cells".format(best, dx, dy))
    lines.append("  that offset in pixels      : {0:+.1f},{1:+.1f}".format(px, py))
    lines.append("  mean difference thereafter : {0:.1f} of 255, maximum {1:.0f}".format(
        float(diff.mean()), float(diff.max())))

    composite = compose(reference, aligned, capture, diff, ref_coarse, cap_coarse,
                        os.path.join(args.out or os.path.dirname(os.path.abspath(args.capture)),
                                     args.name))
    lines.append("")
    lines.append("composite: " + composite)
    for line in lines:
        print(line)

    if args.min_zncc is not None and best < args.min_zncc:
        print("verdict: {0:+.3f} is below --min-zncc {1:+.3f}".format(best, args.min_zncc))
        return 1
    print("verdict: measured, best correlation {0:+.3f}".format(best))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
