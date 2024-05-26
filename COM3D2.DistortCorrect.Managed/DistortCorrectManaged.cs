using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CM3D2.ExternalSaveData.Managed;

namespace CM3D2.DistortCorrect.Managed {
	public class DistortCorrectManaged {
		static HashSet<string> scaleBoneHash = new HashSet<string> {
			"Bip01 L UpperArm",
			"Bip01 L Forearm",
			"Bip01 L Hand",
			"Bip01 L Finger0",
			"Bip01 L Finger01",
			"Bip01 L Finger02",
			"Bip01 L Finger1",
			"Bip01 L Finger11",
			"Bip01 L Finger12",
			"Bip01 L Finger2",
			"Bip01 L Finger21",
			"Bip01 L Finger22",
			"Bip01 L Finger3",
			"Bip01 L Finger31",
			"Bip01 L Finger32",
			"Bip01 L Finger4",
			"Bip01 L Finger41",
			"Bip01 L Finger42",
			"Bip01 L Calf",
			"Bip01 R UpperArm",
			"Bip01 R Forearm",
			"Bip01 R Hand",
			"Bip01 R Finger0",
			"Bip01 R Finger01",
			"Bip01 R Finger02",
			"Bip01 R Finger1",
			"Bip01 R Finger11",
			"Bip01 R Finger12",
			"Bip01 R Finger2",
			"Bip01 R Finger21",
			"Bip01 R Finger22",
			"Bip01 R Finger3",
			"Bip01 R Finger31",
			"Bip01 R Finger32",
			"Bip01 R Finger4",
			"Bip01 R Finger41",
			"Bip01 R Finger42",
			"Bip01 R Calf"
		};
		static Dictionary<string, Vector3> originalBoneDic = null;
		static Dictionary<string, List<BoneMorph.BoneProp>> originalBoneDic2 = null;
		static Dictionary<string, Vector3> newBoneDic = null;
		static Dictionary<string, List<BoneMorph.BoneProp>> newBoneDic2 = null;

		static Dictionary<Maid, bool> limbFixDic = new Dictionary<Maid, bool>();

		public static void AddItem(TBody tb, MPN mpn, string slotname, string filename, string AttachSlot, string AttachName, bool f_bTemp, int version) {
			ResetBoneDic(tb.maid, true);
		}

		public static void DelItem(TBody tb, MPN mpn, string slotname) {
			ResetBoneDic(tb.maid, true);
		}

		public static void PreBlend(BoneMorph_ bm) {
			TryGetMaid(bm, out var maid);
			ResetBoneDic(maid, false);
		}

		public static void ResetBoneDic(Maid maid, bool staticFlg) {
			if (originalBoneDic == null) {
				InitBoneDic();
			}

			if (maid == null) {
				return;
			}

			bool wideSlider = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "WIDESLIDER", false);
			bool limbFix = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "LIMBSFIX", false);
			bool enable = wideSlider && limbFix;
			if (staticFlg || (!limbFixDic.ContainsKey(maid) || (limbFixDic[maid] != enable))) {
				if (enable) {
					BoneMorph.dic = newBoneDic;
					BoneMorph.dic2 = newBoneDic2;
				} else {
					BoneMorph.dic = originalBoneDic;
					BoneMorph.dic2 = originalBoneDic2;
				}
				maid.body0.bonemorph.Init();
				maid.body0.bonemorph.AddRoot(maid.body0.m_Bones.transform);
				limbFixDic[maid] = enable;
			}
		}

		static void InitBoneDic() {
			originalBoneDic = BoneMorph.dic;
			originalBoneDic2 = BoneMorph.dic2;

			BoneMorph.dic = new Dictionary<string, Vector3>();
			BoneMorph.dic2 = new Dictionary<string, List<BoneMorph.BoneProp>>();

			BoneMorph.SetPosition("KubiScl", "Bip01 Neck", 0.95f, 1f, 1f, 1.05f, 1f, 1f);
			BoneMorph.SetPosition("KubiScl", "Bip01 Head", 0.8f, 1f, 1f, 1.2f, 1f, 1f);

			SetUdeScale("UdeScl", 0.85f, 1f, 1f, 1.15f, 1f, 1f);

			BoneMorph.SetScale("EyeSclX", "Eyepos_L", 1f, 1f, 0.92f, 1f, 1f, 1.08f);
			BoneMorph.SetScale("EyeSclX", "Eyepos_R", 1f, 1f, 0.92f, 1f, 1f, 1.08f);
			BoneMorph.SetScale("EyeSclY", "Eyepos_L", 1f, 0.92f, 1f, 1f, 1.08f, 1f);
			BoneMorph.SetScale("EyeSclY", "Eyepos_R", 1f, 0.92f, 1f, 1f, 1.08f, 1f);
			BoneMorph.SetPosition("EyePosX", "Eyepos_R", 1f, 1f, 0.9f, 1f, 1f, 1.1f);
			BoneMorph.SetPosition("EyePosX", "Eyepos_L", 1f, 1f, 0.9f, 1f, 1f, 1.1f);
			BoneMorph.SetPosition("EyePosY", "Eyepos_R", 1f, 0.93f, 1f, 1f, 1.07f, 1f);
			BoneMorph.SetPosition("EyePosY", "Eyepos_L", 1f, 0.93f, 1f, 1f, 1.07f, 1f);
			BoneMorph.SetScale("HeadX", "Bip01 Head", 1f, 0.9f, 0.8f, 1f, 1.1f, 1.2f);
			BoneMorph.SetScale("HeadY", "Bip01 Head", 0.8f, 0.9f, 1f, 1.2f, 1.1f, 1f);

			BoneMorph.SetPosition("DouPer", "Bip01 Spine", 1f, 1f, 0.94f, 1f, 1f, 1.06f);
			BoneMorph.SetPosition("DouPer", "Bip01 Spine0a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
			BoneMorph.SetPosition("DouPer", "Bip01 Spine1", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
			BoneMorph.SetPosition("DouPer", "Bip01 Spine1a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
			BoneMorph.SetPosition("DouPer", "Bip01 Neck", 1.03f, 1f, 1f, 0.97f, 1f, 1f);
			BoneMorph.SetPosition("DouPer", "Bip01 ? Calf", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetPosition("DouPer", "Bip01 ? Foot", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetScale("DouPer", "Bip01 ? Thigh_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetScale("DouPer", "momotwist_?", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetScale("DouPer", "Bip01 ? Calf_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			SetUdeScale("DouPer", 0.98f, 1f, 1f, 1.02f, 1f, 1f);

			BoneMorph.SetPosition("sintyou", "Bip01 Spine", 1f, 1f, 0.85f, 1f, 1f, 1.15f);
			BoneMorph.SetPosition("sintyou", "Bip01 Spine0a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
			BoneMorph.SetPosition("sintyou", "Bip01 Spine1", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
			BoneMorph.SetPosition("sintyou", "Bip01 Spine1a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
			BoneMorph.SetPosition("sintyou", "Bip01 Neck", 0.97f, 1f, 1f, 1.03f, 1f, 1f);
			BoneMorph.SetPosition("sintyou", "Bip01 Head", 0.9f, 1f, 1f, 1.1f, 1f, 1f);
			BoneMorph.SetPosition("sintyou", "Bip01 ? Calf", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetPosition("sintyou", "Bip01 ? Foot", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Thigh_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "momotwist_?", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Calf_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
			SetUdeScale("sintyou", 0.9f, 1f, 1f, 1.1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Thigh", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "momoniku_?", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 Pelvis_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Calf", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Foot", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Skirt", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 Spine_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 Spine0a_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 Spine1_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 Spine1a_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 Spine1a", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Clavicle", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Clavicle_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? UpperArm", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Forearm", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 ? Hand", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Kata_?", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Mune_?", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Mune_?_sub", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("sintyou", "Bip01 Neck_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);


			BoneMorph.SetScale("koshi", "Bip01 Pelvis_SCL_", 1f, 0.8f, 0.92f, 1f, 1.2f, 1.08f);
			BoneMorph.SetScale("koshi", "Bip01 Spine_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
			BoneMorph.SetScale("koshi", "Hip_?", 1f, 0.96f, 0.9f, 1f, 1.04f, 1.1f);
			BoneMorph.SetScale("koshi", "Skirt", 1f, 0.85f, 0.88f, 1f, 1.2f, 1.12f);
			BoneMorph.SetPosition("kata", "Bip01 ? Clavicle", 0.98f, 1f, 0.5f, 1.02f, 1f, 1.5f);
			BoneMorph.SetScale("kata", "Bip01 Spine1a_SCL_", 1f, 1f, 0.95f, 1f, 1f, 1.05f);
			BoneMorph.SetScale("west", "Bip01 Spine_SCL_", 1f, 0.95f, 0.9f, 1f, 1.05f, 1.1f);
			BoneMorph.SetScale("west", "Bip01 Spine0a_SCL_", 1f, 0.85f, 0.7f, 1f, 1.15f, 1.3f);
			BoneMorph.SetScale("west", "Bip01 Spine1_SCL_", 1f, 0.9f, 0.85f, 1f, 1.1f, 1.15f);
			BoneMorph.SetScale("west", "Bip01 Spine1a_SCL_", 1f, 0.95f, 0.95f, 1f, 1.05f, 1.05f);
			BoneMorph.SetScale("west", "Skirt", 1f, 0.92f, 0.88f, 1f, 1.08f, 1.12f);

			newBoneDic = BoneMorph.dic;
			newBoneDic2 = BoneMorph.dic2;

			//foreach (string str in newBoneDic.Keys) {
			//    if (!originalBoneDic.ContainsKey(str)) {
			//        originalBoneDic.Add(str, Vector3.one);
			//    }
			//}

			newBoneDic.Keys
				.Where(str => !originalBoneDic.ContainsKey(str))
				.ToList()
				.ForEach(str => originalBoneDic.Add(str, Vector3.one));

			foreach (KeyValuePair<string, List<BoneMorph.BoneProp>> kvp in newBoneDic2) {
				if (!originalBoneDic2.ContainsKey(kvp.Key)) {
					originalBoneDic2.Add(kvp.Key, new List<BoneMorph.BoneProp>());
				}
				List<BoneMorph.BoneProp> propList = originalBoneDic2[kvp.Key];
				foreach (BoneMorph.BoneProp prop in kvp.Value) {
					string propName = prop.strProp;
					if (!propList.Exists(p => p.strProp == propName)) {
						BoneMorph.BoneProp bp = new BoneMorph.BoneProp();
						bp.strProp = prop.strProp;
						bp.nIndex = prop.nIndex;
						bp.bExistP = prop.bExistP;
						bp.bExistM = prop.bExistM;
						bp.vMinP = Vector3.one;
						bp.vMaxP = Vector3.one;
						bp.vMinM = Vector3.one;
						bp.vMaxM = Vector3.one;
						propList.Add(bp);
					}
				}
			}
		}

		static void SetUdeScale(string tag, float x, float y, float z, float x2, float y2, float z2) {
			BoneMorph.SetScale(tag, "Bip01 ? UpperArm_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Uppertwist_?", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Uppertwist1_?", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Uppertwist1_?", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Forearm", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Forearm_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Foretwist_?", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Foretwist1_?", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Foretwist_?", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Hand", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Hand_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger0", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger1", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger2", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger3", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger4", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger0_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger01", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger01_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger02", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger02_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger1_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger11", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger11_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger12", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger12_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger2_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger21", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger21_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger22", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger22_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger3_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger31", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger31_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger32", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger32_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger4_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger41", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger41_SCL_", x, y, z, x2, y2, z2);
			BoneMorph.SetPosition(tag, "Bip01 ? Finger42", x, y, z, x2, y2, z2);
			BoneMorph.SetScale(tag, "Bip01 ? Finger42_SCL_", x, y, z, x2, y2, z2);
		}

		public static bool JudgeSclBone(bool flg, GameObject bone) {
			if (flg) {
				return true;
			}

			if (scaleBoneHash.Contains(bone.name)) {
				return true;
			}

			return false;
		}

		// BoneMorph_を手がかりに、Maidを得る
		private static bool TryGetMaid(BoneMorph_ boneMorph_, out Maid maid) {
			maid = null;
			if (boneMorph_ == null) {
				return false;
			}
			maid = GetMaids().FirstOrDefault(e => e.body0?.bonemorph != null && e.body0.bonemorph == boneMorph_);
			return maid;
		}

		private static IEnumerable<Maid> GetMaids() {
			var characterManager = GameMain.Instance.CharacterMgr;
			for (var i = 0; i < characterManager.GetStockMaidCount(); i++) {
				yield return characterManager.GetStockMaid(i);
			}
		}
	}
}