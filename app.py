import os
import streamlit as st
from transcriber import transcribe_audio, get_supported_languages
from utils import save_transcript_files
from youtube_downloader import download_youtube_audio
from audio_utils import extract_audio_from_video
import tempfile

st.set_page_config(page_title="Transcription & Translation Tool", layout="centered")
st.title("🎧 AI Audio/Video Transcriber & Translator")

uploaded_file = st.file_uploader("Upload audio/video file", type=["mp3", "mp4", "wav", "m4a"])
youtube_url = st.text_input("Or enter a YouTube URL")

language_map = get_supported_languages()
language_names = ["Auto Detect"] + list(language_map.values())
selected_lang = st.selectbox("Transcription Language", options=language_names)

translation_enabled = st.checkbox("Translate output")
translation_lang = None
if translation_enabled:
    translation_lang = st.selectbox("Select Translation Language", options=list(language_map.values()))

if st.button("Process"):
    audio_path = None

    with st.spinner("Loading audio..."):
        if uploaded_file:
            audio_path = os.path.join(tempfile.mkdtemp(), uploaded_file.name)
            with open(audio_path, "wb") as f:
                f.write(uploaded_file.read())
        elif youtube_url:
            audio_path = download_youtube_audio(youtube_url)
        else:
            st.warning("Please upload a file or provide a YouTube link.")

    if audio_path:
        if audio_path.endswith(".mp4"):
            audio_path = extract_audio_from_video(audio_path)
            if audio_path is None:
                st.error("Audio extraction failed.")
                st.stop()

        lang_code = None if selected_lang == "Auto Detect" else [k for k, v in language_map.items() if v == selected_lang][0]

        progress = st.progress(0)
        status = st.empty()

        def update_progress(pct, msg):
            progress.progress(int(pct * 100))
            status.text(msg)

        segments = transcribe_audio(audio_path, language=lang_code, translate=translation_enabled, on_progress=update_progress)

        full_text = " ".join([seg["text"] for seg in segments])
        st.subheader("📄 Transcription")
        st.text_area("Text", value=full_text, height=250)

        st.subheader("💾 Export")
        export_format = st.selectbox("Choose format", ["SRT", "VTT", "JSON", "CSV"])
        file_path = save_transcript_files(segments, export_format)
        st.download_button("Download", open(file_path, "rb"), file_name=os.path.basename(file_path))
