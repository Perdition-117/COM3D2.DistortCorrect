using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.DistortCorrect.Plugin {
	class BodyBone {
		public List<string> ClavicleScaleList;
		public List<string> ClaviclePositionList;

		public List<string> upperArmScaleList;
		public List<string> upperArmPositionList;

		public List<string> foreArmScaleList;
		public List<string> foreArmPositionList;

		public List<string> handScaleList;
		public List<string> handPositionList;

		public BodyBone() {
			handScaleList = new List<string> {
				"Bip01 ? Hand_SCL_",
				"Bip01 ? Finger0_SCL_",
				"Bip01 ? Finger01_SCL_",
				"Bip01 ? Finger02_SCL_",
				"Bip01 ? Finger1_SCL_",
				"Bip01 ? Finger11_SCL_",
				"Bip01 ? Finger12_SCL_",
				"Bip01 ? Finger2_SCL_",
				"Bip01 ? Finger21_SCL_",
				"Bip01 ? Finger22_SCL_",
				"Bip01 ? Finger3_SCL_",
				"Bip01 ? Finger31_SCL_",
				"Bip01 ? Finger32_SCL_",
				"Bip01 ? Finger4_SCL_",
				"Bip01 ? Finger41_SCL_",
				"Bip01 ? Finger42_SCL_"
			};

			foreArmScaleList = new List<string> {
				"Bip01 ? Forearm_SCL_",
				"Foretwist_?",
				"Foretwist1_?"
			};
			foreArmScaleList.AddRange(handScaleList);

			upperArmScaleList = new List<string> {
				"Bip01 ? UpperArm_SCL_",
				"Uppertwist_?",
				"Uppertwist1_?"
			};
			upperArmScaleList.AddRange(foreArmScaleList);

			ClavicleScaleList = new List<string> {
				"Bip01 ? Clavicle_SCL_",
				"Kata_?"
			};
			ClavicleScaleList.AddRange(upperArmScaleList);

			handPositionList = new List<string> {
				"Bip01 ? Finger0",
				"Bip01 ? Finger01",
				"Bip01 ? Finger02",
				"Bip01 ? Finger1",
				"Bip01 ? Finger11",
				"Bip01 ? Finger12",
				"Bip01 ? Finger2",
				"Bip01 ? Finger21",
				"Bip01 ? Finger22",
				"Bip01 ? Finger3",
				"Bip01 ? Finger31",
				"Bip01 ? Finger32",
				"Bip01 ? Finger4",
				"Bip01 ? Finger41",
				"Bip01 ? Finger42"
			};

			foreArmPositionList = new List<string> {
				"Foretwist_?",
				"Foretwist1_?",
				"Bip01 ? Hand"
			};
			foreArmPositionList.AddRange(handPositionList);

			upperArmPositionList = new List<string> {
				"Uppertwist_?",
				"Uppertwist1_?",
				"Bip01 ? Forearm"
			};
			upperArmPositionList.AddRange(foreArmPositionList);

			ClaviclePositionList = new List<string> {
				"Bip01 ? UpperArm",
				"Kata_?"
			};
			ClaviclePositionList.AddRange(upperArmPositionList);
		}
	}
}
