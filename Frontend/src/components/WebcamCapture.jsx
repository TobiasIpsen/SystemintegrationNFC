import { useEffect, useRef, useState } from 'react';
import { Box, Button, Stack, Typography, Alert } from '@mui/material';
import CameraAltIcon from '@mui/icons-material/CameraAlt';
import ReplayIcon from '@mui/icons-material/Replay';

// Shows a live camera preview, lets the user snap a still frame, and
// returns it as a JPEG data URL via onCapture. That data URL is exactly
// what an <img> or MUI <Avatar src="..."> expects, so nothing else in
// the app needs to change to display it.
export default function WebcamCapture({ onCapture }) {
  const videoRef = useRef(null);
  const streamRef = useRef(null);
  const [error, setError] = useState('');
  const [ready, setReady] = useState(false);

  useEffect(() => {
    let cancelled = false;

    navigator.mediaDevices
      ?.getUserMedia({ video: { facingMode: 'user' }, audio: false })
      .then((stream) => {
        if (cancelled) {
          stream.getTracks().forEach((t) => t.stop());
          return;
        }
        streamRef.current = stream;
        if (videoRef.current) {
          videoRef.current.srcObject = stream;
          setReady(true);
        }
      })
      .catch(() => setError('Could not access the camera. Check permissions and try again.'));

    return () => {
      cancelled = true;
      streamRef.current?.getTracks().forEach((t) => t.stop());
    };
  }, []);

  const capture = () => {
    const video = videoRef.current;
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext('2d').drawImage(video, 0, 0);
    onCapture(canvas.toDataURL('image/jpeg', 0.85));
  };

  if (error) return <Alert severity="warning">{error}</Alert>;

  return (
    <Stack spacing={1.5} alignItems="center">
      <Box
        sx={{
          width: 240, height: 180, borderRadius: 2, overflow: 'hidden',
          bgcolor: '#000', display: 'flex', alignItems: 'center', justifyContent: 'center',
        }}
      >
        <video ref={videoRef} autoPlay playsInline muted style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
      </Box>
      {!ready && <Typography variant="caption" color="text.secondary">Starting camera…</Typography>}
      <Button variant="contained" startIcon={<CameraAltIcon />} disabled={!ready} onClick={capture}>
        Take Photo
      </Button>
    </Stack>
  );
}

// A small preview + "Retake" control, used once a photo has been captured.
export function CapturedPreview({ dataUrl, onRetake }) {
  return (
    <Stack spacing={1.5} alignItems="center">
      <Box
        component="img" src={dataUrl} alt="Captured"
        sx={{ width: 240, height: 180, borderRadius: 2, objectFit: 'cover' }}
      />
      <Button variant="outlined" startIcon={<ReplayIcon />} onClick={onRetake}>
        Retake
      </Button>
    </Stack>
  );
}
