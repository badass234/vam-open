using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battlehub.Cubeman;
using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles
{
	public class CubemenGame : Game
	{
		public Text TxtScore;

		public Text TxtCompleted;

		public Text TxtTip;

		private int m_score;

		private int m_total;

		private bool m_gameOver;

		private List<GameCharacter> m_cubemans;

		private GameCharacter m_current;

		private RTHandlesDemoSmoothFollow m_playerCamera;

		protected override void AwakeOverride()
		{
			m_playerCamera = Object.FindObjectOfType<RTHandlesDemoSmoothFollow>();
			StartGame();
		}

		private void StartGame()
		{
			m_gameOver = false;
			m_playerCamera = Object.FindObjectOfType<RTHandlesDemoSmoothFollow>();
			if (m_playerCamera != null)
			{
				Canvas componentInChildren = GetComponentInChildren<Canvas>();
				Camera camera = (componentInChildren.worldCamera = m_playerCamera.GetComponent<Camera>());
				componentInChildren.planeDistance = camera.nearClipPlane + 0.01f;
			}
			m_cubemans = new List<GameCharacter>();
			CubemanUserControl[] array = (from c in Object.FindObjectsOfType<CubemanUserControl>()
				orderby c.name
				select c).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Rigidbody component2 = array[i].GetComponent<Rigidbody>();
				if ((bool)component2)
				{
					component2.isKinematic = false;
				}
				CubemanCharacter component3 = array[i].GetComponent<CubemanCharacter>();
				if ((bool)component3)
				{
					component3.Enabled = true;
				}
				GameCharacter gameCharacter = array[i].GetComponent<GameCharacter>();
				if (gameCharacter == null)
				{
					gameCharacter = array[i].gameObject.AddComponent<GameCharacter>();
				}
				if (gameCharacter != null)
				{
					gameCharacter.Game = this;
				}
				if (m_playerCamera != null)
				{
					gameCharacter.Camera = m_playerCamera.transform;
				}
				m_cubemans.Add(gameCharacter);
			}
			Begin();
		}

		protected override void OnActiveWindowChanged()
		{
			TryActivateCharacter();
		}

		private void TryActivateCharacter()
		{
			if (m_current != null)
			{
				m_current.IsActive = RuntimeEditorApplication.ActiveWindowType == RuntimeWindowType.GameView || !RuntimeEditorApplication.IsOpened;
			}
		}

		private void Update()
		{
			if (RuntimeEditorApplication.IsActiveWindow(RuntimeWindowType.GameView) || !RuntimeEditorApplication.IsOpened)
			{
				if (InputController.GetKeyDown(KeyCode.Return))
				{
					SwitchPlayer(m_current, 0f, true);
				}
				else if (InputController.GetKeyDown(KeyCode.Backspace))
				{
					SwitchPlayer(m_current, 0f, false);
				}
			}
		}

		private void UpdateScore()
		{
			TxtScore.text = "Saved : " + m_score + " / " + m_total;
		}

		private bool IsGameCompleted()
		{
			return m_cubemans.Count == 0;
		}

		private void Begin()
		{
			m_total = m_cubemans.Count;
			m_score = 0;
			if (m_total == 0)
			{
				TxtCompleted.gameObject.SetActive(true);
				TxtScore.gameObject.SetActive(false);
				TxtTip.gameObject.SetActive(false);
				TxtCompleted.text = "Game Over!";
				m_gameOver = true;
			}
			else
			{
				TxtCompleted.gameObject.SetActive(false);
				TxtScore.gameObject.SetActive(true);
				UpdateScore();
				SwitchPlayer(null, 0f, true);
			}
		}

		public void OnPlayerFinish(GameCharacter gameCharacter)
		{
			m_score++;
			UpdateScore();
			SwitchPlayer(gameCharacter, 1f, true);
			m_cubemans.Remove(gameCharacter);
			if (IsGameCompleted())
			{
				m_gameOver = true;
				TxtTip.gameObject.SetActive(false);
				StartCoroutine(ShowText("Congratulation! \n You have completed a great game "));
			}
		}

		private IEnumerator ShowText(string text)
		{
			yield return new WaitForSeconds(1.5f);
			if (m_gameOver)
			{
				TxtScore.gameObject.SetActive(false);
				TxtCompleted.gameObject.SetActive(true);
				TxtCompleted.text = text;
			}
		}

		public void OnPlayerDie(GameCharacter gameCharacter)
		{
			m_gameOver = true;
			m_cubemans.Remove(gameCharacter);
			TxtTip.gameObject.SetActive(false);
			StartCoroutine(ShowText("Game Over!"));
			for (int i = 0; i < m_cubemans.Count; i++)
			{
				m_cubemans[i].IsActive = false;
			}
		}

		public void SwitchPlayer(GameCharacter current, float delay, bool next)
		{
			if (m_gameOver)
			{
				return;
			}
			int num = 0;
			if (current != null)
			{
				current.IsActive = false;
				num = m_cubemans.IndexOf(current);
				if (next)
				{
					num++;
					if (num >= m_cubemans.Count)
					{
						num = 0;
					}
				}
				else
				{
					num--;
					if (num < 0)
					{
						num = m_cubemans.Count - 1;
					}
				}
			}
			m_current = m_cubemans[num];
			if (current == null)
			{
				ActivatePlayer();
			}
			else
			{
				StartCoroutine(ActivateNextPlayer(delay));
			}
		}

		private IEnumerator ActivateNextPlayer(float delay)
		{
			yield return new WaitForSeconds(delay);
			if (!m_gameOver)
			{
				ActivatePlayer();
			}
		}

		private void ActivatePlayer()
		{
			TryActivateCharacter();
			if (m_playerCamera != null)
			{
				m_playerCamera.target = m_current.transform;
			}
		}
	}
}
