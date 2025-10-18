using System;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System.IO;
using Duckov.Economy;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;

namespace ItemPriceMultiplier
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private bool _isInit = false;
        private Harmony? _harmony = null;
        public static int ItemPriceMultiplierValue { get; private set; } = 2;
        public static bool ShowPrice { get; private set; } = true;

        TextMeshProUGUI? _text = null;

        TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                {
                    _text = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
                }

                return _text;
            }
        }

        protected override void OnAfterSetup()
        {
            Debug.Log("ItemPriceMultiplier模组：OnAfterSetup方法被调用");
            if (!_isInit)
            {
                LoadConfig();
                Debug.Log("ItemPriceMultiplier模组：执行修补");
                _harmony = new Harmony("Lexcellent.ItemPriceMultiplier");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                _isInit = true;
                Debug.Log("ItemPriceMultiplier模组：修补完成");
            }
        }

        protected override void OnBeforeDeactivate()
        {
            Debug.Log("ItemPriceMultiplier模组：OnBeforeDeactivate方法被调用");
            if (_isInit)
            {
                Debug.Log("ItemPriceMultiplier模组：执行取消修补");
                if (_harmony != null)
                {
                    _harmony.UnpatchAll();
                }

                Debug.Log("ItemPriceMultiplier模组：执行取消修补完毕");
            }
        }

        private void LoadConfig()
        {
            try
            {
                string configPath =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "info.ini");
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("ItemPriceMultiplier="))
                        {
                            string value = line.Substring("ItemPriceMultiplier=".Length).Trim();
                            if (int.TryParse(value, out int multiplier))
                            {
                                ItemPriceMultiplierValue = multiplier;
                                Debug.Log($"ItemPriceMultiplier模组：已从配置文件读取ItemPriceMultiplier值: {multiplier}");
                            }
                        }
                        else if (line.StartsWith("ShowPrice="))
                        {
                            string value = line.Substring("ShowPrice=".Length).Trim();
                            if (bool.TryParse(value, out bool showPrice))
                            {
                                ShowPrice = showPrice;
                                Debug.Log($"ItemPriceMultiplier模组：已从配置文件读取ShowPrice值: {showPrice}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("ItemPriceMultiplier模组：未找到info.ini文件，使用默认值");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"ItemPriceMultiplier模组：读取配置文件时出错：{e.Message}，使用默认值");
            }
        }

        void Awake()
        {
            Debug.Log("DisplayItemValue Loaded!!!");
        }

        void OnDestroy()
        {
            if (_text != null)
                Destroy(_text);
        }

        void OnEnable()
        {
            ItemHoveringUI.onSetupItem += OnSetupItemHoveringUI;
        }

        void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnSetupItemHoveringUI;
        }

        private void OnSetupItemHoveringUI(ItemHoveringUI uiInstance, Item item)
        {
            if (!ShowPrice || item == null)
            {
                Text.gameObject.SetActive(false);
                return;
            }

            Text.gameObject.SetActive(true);
            Text.transform.SetParent(uiInstance.LayoutParent);
            Text.transform.localScale = Vector3.one;
            float num = 0.5f;
            var convertPrice = Mathf.FloorToInt((float)item.GetTotalRawValue() * num);
            Text.text = $"${convertPrice * ItemPriceMultiplierValue}";
            Text.fontSize = 20f;
            Text.color = Color.green;
        }
    }

    // 单独的补丁类
    [HarmonyPatch(typeof(StockShop), "ConvertPrice")]
    public static class StockShopPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref int __result, bool selling)
        {
            try
            {
                Debug.Log($"ItemPriceMultiplier模组：模式：{selling}，价格：{__result}");
                if (selling)
                {
                    __result *= ModBehaviour.ItemPriceMultiplierValue;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"ItemPriceMultiplier模组：错误：{e.Message}");
            }
        }
    }
}