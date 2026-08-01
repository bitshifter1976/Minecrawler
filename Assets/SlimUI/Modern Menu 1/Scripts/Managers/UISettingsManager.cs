using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

namespace SlimUI.ModernMenu{
	public class UISettingsManager : MonoBehaviour {

		public enum Platform {Desktop, Mobile};
		public Platform platform;
		// toggle buttons
		[Header("MOBILE SETTINGS")]
		public GameObject mobileSFXtext;
		public GameObject mobileMusictext;
		public GameObject mobileShadowofftextLINE;
		public GameObject mobileShadowlowtextLINE;
		public GameObject mobileShadowhightextLINE;

		[Header("VIDEO SETTINGS")]
		public GameObject fullscreentext;
		public GameObject ambientocclusiontext;
		public GameObject shadowofftextLINE;
		public GameObject shadowlowtextLINE;
		public GameObject shadowhightextLINE;
		public GameObject aaofftextLINE;
		public GameObject aa2xtextLINE;
		public GameObject aa4xtextLINE;
		public GameObject aa8xtextLINE;
		public GameObject vsynctext;
		public GameObject motionblurtext;
		public GameObject texturelowtextLINE;
		public GameObject texturemedtextLINE;
		public GameObject texturehightextLINE;
		public GameObject cameraeffectstext; 

		[Header("GAME SETTINGS")]
		public GameObject showhudtext;
		public GameObject tooltipstext;
        public GameObject difficultyeasyltext;
        public GameObject difficultyeasytextLINE;
        public GameObject difficultynormaltext;
		public GameObject difficultynormaltextLINE;
		public GameObject difficultyhardcoretext;
		public GameObject difficultyhardcoretextLINE;

		[Header("CONTROLS SETTINGS")]
		public GameObject invertmousetext;

		// sliders
		public GameObject musicSlider;
        public GameObject sfxSlider;
        public GameObject sensitivityXSlider;
		public GameObject sensitivityYSlider;
		public GameObject mouseSmoothSlider;
        public GameObject resolutionDropDown;

        //private float sliderValue = 0.0f;
        //private float sliderValueXSensitivity = 0.0f;
        //private float sliderValueYSensitivity = 0.0f;
        //private float sliderValueSmoothing = 0.0f;


        public void  Start (){
			// check difficulty
			if(PlayerPrefs.GetInt("NormalDifficulty") == 1){
				difficultyeasytextLINE.gameObject.SetActive(false);
                difficultynormaltextLINE.gameObject.SetActive(true);
				difficultyhardcoretextLINE.gameObject.SetActive(false);
			}
			else if (PlayerPrefs.GetInt("HardCoreDifficulty") == 1)
            {
				difficultyeasytextLINE.gameObject.SetActive(false);
                difficultyhardcoretextLINE.gameObject.SetActive(true);
				difficultynormaltextLINE.gameObject.SetActive(false);
			}
			else
            {
                difficultyeasytextLINE.gameObject.SetActive(true);
                difficultynormaltextLINE.gameObject.SetActive(false);
                difficultyhardcoretextLINE.gameObject.SetActive(false);
            }

            // check slider values
            musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
			sfxSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("SFXVolume");
			//sensitivityXSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("XSensitivity");
			//sensitivityYSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("YSensitivity");
			//mouseSmoothSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseSmoothing");

			// check full screen
			var fullscreenPref = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
            if (fullscreenPref == 1)
			{ 
				fullscreentext.GetComponent<TMP_Text>().text = "on";
			}
			else 
			{
				fullscreentext.GetComponent<TMP_Text>().text = "off";
			}

            // check resolution
			var resolutionWidthPref = PlayerPrefs.GetInt("ResolutionWidth", 0);
			var resolutionHeightPref = PlayerPrefs.GetInt("ResolutionHeight", 0);
			var currentResolution = Screen.currentResolution;
            resolutionDropDown.GetComponent<TMP_Dropdown>().value = GetResolutionIndex(resolutionWidthPref, resolutionHeightPref, currentResolution);

            // check hud value
            if (PlayerPrefs.GetInt("ShowHUD")==0)
			{
				showhudtext.GetComponent<TMP_Text>().text = "off";
			}
			else{
				showhudtext.GetComponent<TMP_Text>().text = "on";
			}

			// check tool tip value
			if (PlayerPrefs.GetInt("ToolTips")==0)
			{
				tooltipstext.GetComponent<TMP_Text>().text = "off";
			}
			else{
				tooltipstext.GetComponent<TMP_Text>().text = "on";
			}
		}

        private int GetResolutionIndex(int resolutionWidthPref, int resolutionHeightPref, Resolution currentResolution)
        {
            // Implementation for getting resolution index from the dropdown based on saved preferences or current resolution
			foreach (var option in resolutionDropDown.GetComponent<TMP_Dropdown>().options)
            {
                // Compare the resolution values and return the index if they match
                if (option.text == $"{resolutionWidthPref} x {resolutionHeightPref}")
                    return resolutionDropDown.GetComponent<TMP_Dropdown>().options.IndexOf(option);
            }
            return 0;
        }

        public void Update (){
			//sliderValue = musicSlider.GetComponent<Slider>().value;
			//sliderValueXSensitivity = sensitivityXSlider.GetComponent<Slider>().value;
			//sliderValueYSensitivity = sensitivityYSlider.GetComponent<Slider>().value;
			//sliderValueSmoothing = mouseSmoothSlider.GetComponent<Slider>().value;
		}

		public void FullScreen (){
			Screen.fullScreen = !Screen.fullScreen;

			if(Screen.fullScreen == true){
				fullscreentext.GetComponent<TMP_Text>().text = "on";
			}
			else if(Screen.fullScreen == false){
				fullscreentext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void ResolutionDropDown()
        {
            int selectedIndex = resolutionDropDown.GetComponent<TMP_Dropdown>().value;
            Resolution[] resolutions = Screen.resolutions;
            if (selectedIndex >= 0 && selectedIndex < resolutions.Length)
            {
                Resolution selectedResolution = resolutions[selectedIndex];
                Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
                PlayerPrefs.SetInt("ResolutionWidth", selectedResolution.width);
                PlayerPrefs.SetInt("ResolutionHeight", selectedResolution.height);
            }
        }

        public void MusicSlider (){
			//PlayerPrefs.SetFloat("MusicVolume", sliderValue);
			PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetComponent<Slider>().value);
		}

		public void SFXSlider()
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.GetComponent<Slider>().value);
        }

        /*public void SensitivityXSlider (){
			PlayerPrefs.SetFloat("XSensitivity", sliderValueXSensitivity);
		}

		public void SensitivityYSlider (){
			PlayerPrefs.SetFloat("YSensitivity", sliderValueYSensitivity);
		}

		public void SensitivitySmoothing (){
			PlayerPrefs.SetFloat("MouseSmoothing", sliderValueSmoothing);
			Debug.Log(PlayerPrefs.GetFloat("MouseSmoothing"));
		}*/

        // the playerprefs variable that is checked to enable hud while in game
        public void ShowHUD (){
			if(PlayerPrefs.GetInt("ShowHUD")==0){
				PlayerPrefs.SetInt("ShowHUD",1);
				showhudtext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("ShowHUD")==1){
				PlayerPrefs.SetInt("ShowHUD",0);
				showhudtext.GetComponent<TMP_Text>().text = "off";
			}
		}

		// the playerprefs variable that is checked to enable mobile sfx while in game
		public void MobileSFXMute (){
			if(PlayerPrefs.GetInt("Mobile_MuteSfx")==0){
				PlayerPrefs.SetInt("Mobile_MuteSfx",1);
				mobileSFXtext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("Mobile_MuteSfx")==1){
				PlayerPrefs.SetInt("Mobile_MuteSfx",0);
				mobileSFXtext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void MobileMusicMute (){
			if(PlayerPrefs.GetInt("Mobile_MuteMusic")==0){
				PlayerPrefs.SetInt("Mobile_MuteMusic",1);
				mobileMusictext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("Mobile_MuteMusic")==1){
				PlayerPrefs.SetInt("Mobile_MuteMusic",0);
				mobileMusictext.GetComponent<TMP_Text>().text = "off";
			}
		}

		// show tool tips like: 'How to Play' control pop ups
		public void ToolTips (){
			if(PlayerPrefs.GetInt("ToolTips")==0){
				PlayerPrefs.SetInt("ToolTips",1);
				tooltipstext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("ToolTips")==1){
				PlayerPrefs.SetInt("ToolTips",0);
				tooltipstext.GetComponent<TMP_Text>().text = "off";
			}
		}

        public void EasyDifficulty()
        {
            difficultyeasytextLINE.gameObject.SetActive(true);
            difficultynormaltextLINE.gameObject.SetActive(false);
			difficultyhardcoretextLINE.gameObject.SetActive(false);
            PlayerPrefs.SetInt("NormalDifficulty", 0);
            PlayerPrefs.SetInt("HardCoreDifficulty", 0);
			PlayerPrefs.SetInt("EasyDifficulty", 1);
        }

        public void NormalDifficulty (){
			difficultyhardcoretextLINE.gameObject.SetActive(false);
			difficultynormaltextLINE.gameObject.SetActive(true);
			difficultyeasytextLINE.gameObject.SetActive(false);
            PlayerPrefs.SetInt("NormalDifficulty", 1);
			PlayerPrefs.SetInt("HardCoreDifficulty", 0);
            PlayerPrefs.SetInt("EasyDifficulty", 0);
        }

		public void HardcoreDifficulty (){
			difficultyhardcoretextLINE.gameObject.SetActive(true);
			difficultynormaltextLINE.gameObject.SetActive(false);
			difficultyeasytextLINE.gameObject.SetActive(false);
			PlayerPrefs.SetInt("NormalDifficulty", 0);
			PlayerPrefs.SetInt("HardCoreDifficulty", 1);
            PlayerPrefs.SetInt("EasyDifficulty", 0);
        }

		public void ShadowsOff (){
			PlayerPrefs.SetInt("Shadows",0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			shadowofftextLINE.gameObject.SetActive(true);
			shadowlowtextLINE.gameObject.SetActive(false);
			shadowhightextLINE.gameObject.SetActive(false);
		}

		public void ShadowsLow (){
			PlayerPrefs.SetInt("Shadows",1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			shadowofftextLINE.gameObject.SetActive(false);
			shadowlowtextLINE.gameObject.SetActive(true);
			shadowhightextLINE.gameObject.SetActive(false);
		}

		public void ShadowsHigh (){
			PlayerPrefs.SetInt("Shadows",2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			shadowofftextLINE.gameObject.SetActive(false);
			shadowlowtextLINE.gameObject.SetActive(false);
			shadowhightextLINE.gameObject.SetActive(true);
		}

		public void MobileShadowsOff (){
			PlayerPrefs.SetInt("MobileShadows",0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			mobileShadowofftextLINE.gameObject.SetActive(true);
			mobileShadowlowtextLINE.gameObject.SetActive(false);
			mobileShadowhightextLINE.gameObject.SetActive(false);
		}

		public void MobileShadowsLow (){
			PlayerPrefs.SetInt("MobileShadows",1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			mobileShadowofftextLINE.gameObject.SetActive(false);
			mobileShadowlowtextLINE.gameObject.SetActive(true);
			mobileShadowhightextLINE.gameObject.SetActive(false);
		}

		public void MobileShadowsHigh (){
			PlayerPrefs.SetInt("MobileShadows",2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			mobileShadowofftextLINE.gameObject.SetActive(false);
			mobileShadowlowtextLINE.gameObject.SetActive(false);
			mobileShadowhightextLINE.gameObject.SetActive(true);
		}

		public void vsync (){
			if(QualitySettings.vSyncCount == 0){
				QualitySettings.vSyncCount = 1;
				vsynctext.GetComponent<TMP_Text>().text = "on";
			}
			else if(QualitySettings.vSyncCount == 1){
				QualitySettings.vSyncCount = 0;
				vsynctext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void InvertMouse (){
			if(PlayerPrefs.GetInt("Inverted")==0){
				PlayerPrefs.SetInt("Inverted",1);
				invertmousetext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("Inverted")==1){
				PlayerPrefs.SetInt("Inverted",0);
				invertmousetext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void MotionBlur (){
			if(PlayerPrefs.GetInt("MotionBlur")==0){
				PlayerPrefs.SetInt("MotionBlur",1);
				motionblurtext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("MotionBlur")==1){
				PlayerPrefs.SetInt("MotionBlur",0);
				motionblurtext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void AmbientOcclusion (){
			if(PlayerPrefs.GetInt("AmbientOcclusion")==0){
				PlayerPrefs.SetInt("AmbientOcclusion",1);
				ambientocclusiontext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("AmbientOcclusion")==1){
				PlayerPrefs.SetInt("AmbientOcclusion",0);
				ambientocclusiontext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void CameraEffects (){
			if(PlayerPrefs.GetInt("CameraEffects")==0){
				PlayerPrefs.SetInt("CameraEffects",1);
				cameraeffectstext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("CameraEffects")==1){
				PlayerPrefs.SetInt("CameraEffects",0);
				cameraeffectstext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void TexturesLow (){
			PlayerPrefs.SetInt("Textures",0);
			QualitySettings.globalTextureMipmapLimit = 2;
			texturelowtextLINE.gameObject.SetActive(true);
			texturemedtextLINE.gameObject.SetActive(false);
			texturehightextLINE.gameObject.SetActive(false);
		}

		public void TexturesMed (){
			PlayerPrefs.SetInt("Textures",1);
			QualitySettings.globalTextureMipmapLimit = 1;
			texturelowtextLINE.gameObject.SetActive(false);
			texturemedtextLINE.gameObject.SetActive(true);
			texturehightextLINE.gameObject.SetActive(false);
		}

		public void TexturesHigh (){
			PlayerPrefs.SetInt("Textures",2);
			QualitySettings.globalTextureMipmapLimit = 0;
			texturelowtextLINE.gameObject.SetActive(false);
			texturemedtextLINE.gameObject.SetActive(false);
			texturehightextLINE.gameObject.SetActive(true);
		}
	}
}