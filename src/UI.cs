using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ParadoxNotion.Serialization;
using ChaCustom;

using HarmonyLib;

using KKAPI.Maker;

namespace SMAP
{
	public partial class SMAP
	{
		private static CanvasGroup _cgAccessoryTop, _cgAcsParentWindow;
		private static SMAPUI _makerConfigWindow = null;

		private static Toggle _tglSMAP = null;

		internal class SMAPUI : MonoBehaviour
		{
			private ChaControl _chaCtrl = null;
			private int _windowRectID;
			private Vector2 _windowSize = new Vector2(330f, 250f);
			private Rect _windowRect;
			private Texture2D _windowBGtex = null;
			private const int singleItemWidth = 23;

			private static List<string> _boneSuggestions = new List<string>();

			private bool _searchFieldValueChanged = false;
			private string _searchFieldValue = "";
			private string SearchFieldValue
			{
				get => _searchFieldValue;
				set
				{
					if (_searchFieldValue != value)
					{
						_searchFieldValue = value;
						_searchFieldValueChanged = true;
					}
				}
			}

			private static List<string> _bookmark = new List<string>();
			private static string _bookmarkFile = Path.Combine(BepInEx.Paths.ConfigPath, "SMAPBookmark.json");

			private Vector2 _boneScrollPos = Vector2.zero;
			private static int _curSlot => AccessoriesApi.SelectedMakerAccSlot;

			private void Awake()
			{
				DontDestroyOnLoad(this);
				//enabled = false;
				_windowRect = new Rect(525, 636, _windowSize.x, _windowSize.y);

				GameObject _base = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop/AcsParentWindow");
				Toggle _origin = Traverse.Create(_base.GetComponent<CustomAcsParentWindow>()).Field("tglParent").GetValue<Toggle[]>()[0];
				Transform _copy = Instantiate(_origin.transform, _base.transform.Find("grpParent").transform, true);
				RectTransform _copy_rt = _copy.GetComponent<RectTransform>();

				_copy.name = "imgRbColSMAP";
				_copy_rt.offsetMin = new Vector2(284f, -604f);
				_copy_rt.offsetMax = new Vector2(372f, -584f);
				_copy.GetComponentInChildren<TextMeshProUGUI>().text = "S.M.A.P";
				_tglSMAP = _copy.GetComponent<Toggle>();
				_tglSMAP.onValueChanged.RemoveAllListeners();
				_windowBGtex = MakeTex((int) _windowSize.x, (int) _windowSize.y, new Color(0.5f, 0.5f, 0.5f, 1f));

				_chaCtrl = CustomBase.Instance.chaCtrl;
				_windowRectID = GUIUtility.GetControlID(FocusType.Passive);

				_cgAccessoryTop = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop").GetComponent<CanvasGroup>();
				_cgAcsParentWindow = _base.GetComponent<CanvasGroup>();

				AccessoryParentKey = Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey)).Skip(1).Where(x => !x.StartsWith("a_n_")).ToList();
				_boneSuggestions = AccessoryParentKey.ToList();

				LoadBookmark();
			}

			private static void SaveBookmark()
			{
				string json = JSONSerializer.Serialize(typeof(List<string>), _bookmark, true);
				File.WriteAllText(_bookmarkFile, json);
			}

			private static void LoadBookmark()
			{
				if (!File.Exists(_bookmarkFile)) return;
				_bookmark = JSONSerializer.Deserialize<List<string>>(File.ReadAllText(_bookmarkFile));
			}

			private static bool _display()
			{
				if (CustomBase.Instance.customCtrl.hideFrontUI)
					return false;
				if (!Manager.Scene.Instance.AddSceneName.IsNullOrEmpty() && Manager.Scene.Instance.AddSceneName != "CustomScene")
					return false;
				if (_cgAccessoryTop != null && _cgAcsParentWindow != null)
					return (_cgAccessoryTop.alpha > 0 && _cgAcsParentWindow.alpha > 0);
				return false;
			}

			private ChaFileAccessory.PartsInfo GetPartsInfo()
			{
				if (_chaCtrl == null)
					return null;
				if (_curSlot < 20)
				{
					if (_chaCtrl.chaFile.coordinate.ElementAtOrDefault(_chaCtrl.fileStatus.coordinateType) == null)
						return null;
					return _chaCtrl.chaFile.coordinate[_chaCtrl.fileStatus.coordinateType].accessory.parts.ElementAtOrDefault(_curSlot);
				}
				return MoreAccessoriesKOI.MoreAccessories._self._charaMakerData.nowAccessories.ElementAtOrDefault(_curSlot - 20);
			}

			private void OnGUI()
			{
				//if (_chaCtrl == null) return;
				if (!_display()) return;

				if (_searchFieldValueChanged)
				{
					_searchFieldValueChanged = false;
					_boneSuggestions = AccessoryParentKey.ToList();
					if (!_searchFieldValue.IsNullOrEmpty())
						_boneSuggestions = _boneSuggestions.Where(x => x.Contains(_searchFieldValue)).ToList();
				}
				/*
				GUIStyle _windowSolid = new GUIStyle(GUI.skin.window);
				//_windowSolid.normal.background = _windowBGtex;
				_windowSolid.onNormal.background = _windowBGtex;
				_windowSolid.normal.textColor = Color.grey;
				_windowSolid.onNormal.textColor = Color.cyan;

				GUILayout.Window(_windowRectID, _windowRect, DrawWindowContents, $"S.M.A.P - Slot{_curSlot + 1:00}", _windowSolid);
				*/
				GUILayout.Window(_windowRectID, _windowRect, DrawWindowContents, "");

				if (_windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
					Input.ResetInputAxes();
			}

			private void DrawWindowContents(int _windowID)
			{
				GUI.Box(new Rect(0, 0, _windowSize.x, _windowSize.y), _windowBGtex);
				GUI.Box(new Rect(0, 0, _windowSize.x, 23), $"S.M.A.P - Slot{_curSlot + 1:00}", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

				GUIStyle _buttonActive = new GUIStyle(GUI.skin.button);
				_buttonActive.normal.textColor = Color.cyan;
				_buttonActive.hover.textColor = Color.cyan;

				ChaFileAccessory.PartsInfo _partInfo = GetPartsInfo();

				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical(GUILayout.Width(200));
					{
						GUILayout.BeginHorizontal();
						{
							SearchFieldValue = GUILayout.TextField(SearchFieldValue);
							if (SearchFieldValue.IsNullOrEmpty())
								GUI.enabled = false;

							if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
							{
								if (_bookmark.IndexOf(SearchFieldValue) < 0)
								{
									_bookmark.Add(SearchFieldValue);
									SaveBookmark();
								}
							}

							if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
							{
								SearchFieldValue = "";
							}
							GUI.enabled = true;
						}
						GUILayout.EndHorizontal();

						_boneScrollPos = GUILayout.BeginScrollView(_boneScrollPos, GUI.skin.box);
						{
							GUILayout.BeginVertical();
							{
								int leftItemCount = Mathf.FloorToInt(_boneScrollPos.y / singleItemWidth);
								int shownItemCount = 8;

								List<string> _filtered = _boneSuggestions.ToList();
								if (_filtered.Count > (shownItemCount - 1))
								{
									if (_boneSuggestions.Count - leftItemCount < shownItemCount)
										leftItemCount = _boneSuggestions.Count - shownItemCount;

									GUILayout.Space(leftItemCount * singleItemWidth);
									_filtered = _boneSuggestions.Skip(leftItemCount).Take(shownItemCount).ToList();
								}

								foreach (string _name in _filtered)
								{
									GUILayout.BeginHorizontal();
									if (_name == _partInfo?.parentKey)
										GUILayout.Label(_name, _buttonActive);
									else
									{
										if (GUILayout.Button(_name))
										{
											if (_partInfo == null) return;

											_chaCtrl.ChangeAccessoryParent(_curSlot, _name);
											_partInfo.parentKey = _name;
											CustomBase.Instance.updateCustomUI = true;
										}
									}
									GUILayout.EndHorizontal();
								}

								if (_filtered.Count > (shownItemCount - 1))
								{
									int rightItemCount = Mathf.Max(0, _boneSuggestions.Count - (leftItemCount + shownItemCount));
									GUILayout.Space(rightItemCount * singleItemWidth);
								}
							}
							GUILayout.EndVertical();
						}
						GUILayout.EndScrollView();
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
					{
						for (int i = 0; i < _bookmark.Count; i++)
						{
							GUILayout.BeginHorizontal();
							if (GUILayout.Button(_bookmark[i]))
								SearchFieldValue = _bookmark[i];
							if (GUILayout.Button("X", GUILayout.Width(20)))
							{
								_bookmark.Remove(_bookmark[i]);
								SaveBookmark();
							}
							GUILayout.EndHorizontal();
						}
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();

				GUI.DragWindow();
			}

			private Texture2D MakeTex(int _width, int _height, Color _color)
			{
				Color[] pix = new Color[_width * _height];

				for (int i = 0; i < pix.Length; i++)
					pix[i] = _color;

				Texture2D result = new Texture2D(_width, _height);
				result.SetPixels(pix);
				result.Apply();

				return result;
			}
		}
	}
}
