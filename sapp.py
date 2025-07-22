import os
import torch
import whisperx
import streamlit as st
import tempfile
import base64

st.set_page_config(page_title="WhisperX Transcriber", layout="centered")
st.title("🎙️ AI Audio Transcriber")
st.markdown("This tool uses `WhisperX` for fast and accurate transcription.")

# Define a simple language map
lang_map = {
    "English": "en",
    "Hindi": "hi",
    "Bengali": "bn",
    "Japanese": "ja",
    "French": "fr",
    "German": "de"
}

# Input section
uploaded_file = st.file_uploader("Upload Audio/Video File", type=["mp3", "mp4", "wav", "m4a"])
selected_task = st.radio("Task", ["Transcribe", "Translate"])
selected_lang = st.selectbox("Language (for model loading)", list(lang_map.keys()), index=0)

# Output folder
OUTPUT_DIR = os.path.join(os.getcwd(), "outputs")
os.makedirs(OUTPUT_DIR, exist_ok=True)

if uploaded_file:
    st.audio(uploaded_file, format='audio/mp3')
    temp_file = tempfile.NamedTemporaryFile(delete=False, suffix=".mp3")
    temp_file.write(uploaded_file.read())
    temp_file.close()

    if st.button("Start Transcription"):
        with st.spinner("Transcribing with WhisperX..."):
            device = "cuda" if torch.cuda.is_available() else "cpu"
            lang_code = lang_map[selected_lang]

            model = whisperx.load_model("large-v3", device, language=lang_code)

            audio = whisperx.load_audio(temp_file.name)
            result = model.transcribe(audio, batch_size=16, language=lang_code)

            segments = result.get("segments", [])
            if not segments:
                st.error("No speech detected.")
            else:
                st.success("Transcription complete.")
                st.markdown("### Transcript")
                for segment in segments:
                    st.markdown(f"**[{segment['start']:.2f} - {segment['end']:.2f}]** {segment['text']}")

                # Save transcript
                base_name = os.path.splitext(os.path.basename(temp_file.name))[0]
                txt_file = os.path.join(OUTPUT_DIR, base_name + ".txt")
                with open(txt_file, "w", encoding="utf-8") as f:
                    for segment in segments:
                        f.write(f"[{segment['start']:.2f} - {segment['end']:.2f}] {segment['text']}\n")

                with open(txt_file, "rb") as f:
                    b64 = base64.b64encode(f.read()).decode()
                    href = f'<a href="data:file/txt;base64,{b64}" download="{base_name}.txt">📥 Download Transcript</a>'
                    st.markdown(href, unsafe_allow_html=True)

        os.remove(temp_file.name)
