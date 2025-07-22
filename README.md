# 🎧 AI Audio/Video Transcriber & Translator using WhisperX

This is a Streamlit-based app that:
- Transcribes audio/video or YouTube URLs
- Translates into select Indian + global languages
- Exports to SRT, VTT, CSV, or JSON
- Optionally burns subtitles into video
- Supports speaker diarization (if desired)

## 🛠️ Setup Instructions

### 1. Clone this repo
```bash
git clone https://github.com/SwapnoneelBarua/-Audio-Video-Transcriber-Tool---WhisperX
cd WhisperX-Transcriber
```
### Create Conda Environment
Using environment.yml:
```bash
conda env create -f environment.yml
conda activate whisperx_env
```
If environment.yml not available, create manually:
```bash
conda create -n Whisperx_env python=3.10
conda activate Whisperx_env
pip install -r requirements.txt
```
### Launch App
```bash
streamlit run app.py
```
### ✅ Features
1. Multilingual support

2. Translation options

3. YouTube support

4. Export & subtitle burn-in

### 📦 Requirements
```bash
See environment.yml
```
## 📎 License
MIT 
