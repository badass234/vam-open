#!/usr/bin/env python3
"""Measure what a shadow switch does to the body pixels of a capture pair.

Usage:
    python tools/measure_shadow.py <reference> <test> [<test> ...] [options]

`compare_view.py` answers "does this capture match that render" and dilutes any
difference over the whole frame.  When the question is instead "did this light
term darken the surface", the statistic has to be taken where there was light to
lose: the mask is every pixel of the reference whose luminance is above
--threshold, and the delta is `reference - test` on that mask only (positive
means the test is darker).  Reporting the mean over the full frame instead made
a real contact-shadow grid read as "0.6/255, the filter does nothing" -- which is
the measurement error this tool exists to prevent.

The grid breakdown is the other half of the answer: a shadow filter leaves a
spatially structured delta (cells near the contact points darken, cells on open
skin do not), while a global change in exposure or a broken colour space leaves
a uniform one.

Exit codes: 0 measured, 2 bad input.
"""

import argparse
import os
import sys

import numpy as np
from PIL import Image

DEFAULT_THRESHOLD = 32.0
GRID = 8
BANDS = (2.0, 5.0, 10.0, 40.0)


def load_luma(path):
    with Image.open(path) as handle:
        handle.load()
        rgb = np.asarray(handle.convert("RGB"), dtype=np.float32)
    return 0.2126 * rgb[..., 0] + 0.7152 * rgb[..., 1] + 0.0722 * rgb[..., 2], rgb


def percentile(values, q):
    return float(np.percentile(values, q)) if values.size else float("nan")


def measure(reference, test, threshold, grid):
    if reference.shape != test.shape:
        raise ValueError(f"frames differ in size: {reference.shape} vs {test.shape}")
    mask = reference > threshold
    if not mask.any():
        raise ValueError(f"no pixel of the reference is brighter than {threshold:g}")
    delta = (reference - test)[mask]
    height, width = reference.shape
    rows = np.linspace(0, height, grid + 1).astype(int)
    cols = np.linspace(0, width, grid + 1).astype(int)
    cells = np.full((grid, grid), np.nan, dtype=np.float32)
    for r in range(grid):
        for c in range(grid):
            block = mask[rows[r]:rows[r + 1], cols[c]:cols[c + 1]]
            if block.any():
                part = (reference - test)[rows[r]:rows[r + 1], cols[c]:cols[c + 1]]
                cells[r, c] = float(part[block].mean())
    return {
        "mask": mask,
        "delta": delta,
        "cells": cells,
        "mean": float(delta.mean()),
        "median": percentile(delta, 50),
        "p90": percentile(delta, 90),
        "p99": percentile(delta, 99),
        "max": float(delta.max()),
        "min": float(delta.min()),
        "shares": [(band, float((delta > band).mean())) for band in BANDS],
        "deep_pixels": int((delta > BANDS[-1]).sum()),
    }


def report(name, stats, threshold):
    print(f"--- {name} (delta = reference - test, lit pixels only, reference luma > {threshold:g}) ---")
    print(f"  pixels compared: {stats['delta'].size}")
    print(
        f"  mean {stats['mean']:+.2f}  median {stats['median']:+.2f}  "
        f"p90 {stats['p90']:+.2f}  p99 {stats['p99']:+.2f}  "
        f"min {stats['min']:+.2f}  max {stats['max']:+.2f}"
    )
    shares = "  ".join(f">{band:g} {share * 100:4.1f}%" for band, share in stats["shares"])
    print(f"  share of the mask: {shares}  ({stats['deep_pixels']} px deeper than {BANDS[-1]:g})")
    cells = stats["cells"]
    print(f"  mean delta per cell ({cells.shape[0]}x{cells.shape[1]}):")
    for row in cells:
        print("    " + " ".join("   .  " if np.isnan(v) else f"{v:+6.1f}" for v in row))
    flat = cells[~np.isnan(cells)]
    if flat.size:
        print(f"  cell range: {flat.min():+.2f} .. {flat.max():+.2f}, spread {flat.max() - flat.min():.2f}")


def write_composite(path, reference_rgb, test_rgb, stats):
    mask = stats["mask"]
    delta = reference_rgb - test_rgb
    heat = np.zeros_like(reference_rgb)
    magnitude = np.abs(delta).mean(axis=2) * 4.0
    heat[..., 0] = np.clip(magnitude, 0, 255)
    heat[..., 2] = np.clip(magnitude * (delta.mean(axis=2) < 0), 0, 255)
    grey = (0.2126 * reference_rgb[..., 0] + 0.7152 * reference_rgb[..., 1] + 0.0722 * reference_rgb[..., 2])
    outline = np.where(mask, 0.35, 0.0)[..., None] * grey[..., None]
    panels = [
        reference_rgb.astype(np.uint8),
        test_rgb.astype(np.uint8),
        np.clip(reference_rgb * 0.45 + heat, 0, 255).astype(np.uint8),
        (np.where(mask[..., None], delta + 128.0, outline)).clip(0, 255).astype(np.uint8),
    ]
    height, width = reference_rgb.shape[:2]
    canvas = Image.new("RGB", (width * len(panels) + 8 * (len(panels) - 1), height), (24, 24, 24))
    for index, panel in enumerate(panels):
        canvas.paste(Image.fromarray(panel), (index * (width + 8), 0))
    canvas.save(path)


def main(argv):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("reference", help="the brighter frame, e.g. rendered with shadows off")
    parser.add_argument("tests", nargs="+", help="one or more frames to measure against it")
    parser.add_argument("--threshold", type=float, default=DEFAULT_THRESHOLD,
                        help=f"reference luminance above which a pixel counts as lit (default {DEFAULT_THRESHOLD:g})")
    parser.add_argument("--grid", type=int, default=GRID, help=f"cell breakdown size (default {GRID})")
    parser.add_argument("--out", help="write a composite for the eye per test, named <prefix>-<name>.png")
    args = parser.parse_args(argv[1:])

    for path in [args.reference] + args.tests:
        if not os.path.isfile(path):
            print(f"error: {path} is not a file", file=sys.stderr)
            return 2

    reference, reference_rgb = load_luma(args.reference)
    print(f"reference: {args.reference}")
    status = 0
    for path in args.tests:
        try:
            test, test_rgb = load_luma(path)
            stats = measure(reference, test, args.threshold, args.grid)
        except ValueError as error:
            print(f"--- {path} ---")
            print(f"  unusable: {error}")
            status = max(status, 2)
            continue
        name = os.path.splitext(os.path.basename(path))[0]
        report(name, stats, args.threshold)
        if args.out:
            target = f"{args.out}-{name}.png"
            write_composite(target, reference_rgb, test_rgb, stats)
            print(f"  composite: {target}")
    return status


if __name__ == "__main__":
    sys.exit(main(sys.argv))
