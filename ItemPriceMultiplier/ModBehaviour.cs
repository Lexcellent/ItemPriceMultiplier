using System;
using UnityEngine;
using HarmonyLib;
using System.IO;
using Duckov.Economy;
using ItemStatsSystem;

namespace ItemPriceMultiplier
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private bool _isInit = false;
        private string? _harmonyId = null;
        private static int _itemPriceMultiplier = 2;

        protected override void OnAfterSetup()
        {
            Debug.Log("ItemPriceMultiplier模组：OnAfterSetup方法被调用");
            if (!_isInit)
            {
                LoadConfig();
                Debug.Log("ItemPriceMultiplier模组：执行修补");
                _harmonyId = Harmony.CreateAndPatchAll(typeof(ItemPriceMultiplier.ModBehaviour)).Id;
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
                if (_harmonyId != null)
                {
                    Harmony.UnpatchID(_harmonyId);
                }

                Debug.Log("ItemPriceMultiplier模组：执行取消修补完毕");
            }
        }

        private void LoadConfig()
        {
            try
            {
                string configPath =
                    Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
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
                                _itemPriceMultiplier = multiplier;
                                Debug.Log($"ItemPriceMultiplier模组：已从配置文件读取ItemPriceMultiplier值: {multiplier}");
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

        [HarmonyPostfix, HarmonyPatch(typeof(StockShop), "ConvertPrice")]
        public static void ItemPriceMultiplierPatch(ref int __result, bool selling)
        {
            try
            {
                Debug.Log($"ItemPriceMultiplier模组：模式：{selling}，价格：{__result}");
                if (selling)
                {
                    __result *= _itemPriceMultiplier;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"ItemPriceMultiplier模组：错误：{e.Message}");
            }
        }
    }
}