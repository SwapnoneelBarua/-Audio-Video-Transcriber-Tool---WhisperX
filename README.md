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
🎉 Major Update – Subtitle Fix + Vegas Pro Integration 🎬

This v2.0 release of the Audio/Video Transcriber Tool using WhisperX brings powerful enhancements for content creators:

✅ Fixed subtitle sync & missing captions issue  
✅ Tested & verified with **Vegas Pro 14–21** (no plugins needed)  
✅ Generates synced `.srt`, `.vtt`, `.txt`, `.tsv` subtitle files  
✅ Multilingual transcription & translation  
✅ Support for YouTube links and local files  
✅ Works with OFX-compatible video editors

💡 Uses OpenAI WhisperX + Pyannote for speaker diarization  
💬 Includes Streamlit UI and CLI options

🎥 Watch the usage tutorial: [YT](https://youtu.be/tFIOVvJdUnA)
🔉 AI voiceover generated using: [Multimodel-TTS-Synthesizer](https://github.com/SwapnoneelBarua/multimodel-tts-synthesizer)

---
📦 Installation:
```bash
git clone https://github.com/SwapnoneelBarua/-Audio-Video-Transcriber-Tool---WhisperX
cd WhisperX
conda env create -f environment.yml
conda activate whisperx_env
streamlit run app.py
```
### step 2
```bash
Just import your audio or video file into Vegas.
Select the audio track, then run the script. (WhisperX\Video_Subtitle_Script\Whisper Speech To Text.cs)
You’ll get four options — from fast and draft quality to the highest accuracy, plus a translation mode for multilingual subtitles.
The script automatically creates all subtitle files —
.srt, .vtt, .txt, and .tsv — right in the same folder as your media.
And yes, it inserts them directly into a new track in your video project.
```

## 📎 License
MIT 
