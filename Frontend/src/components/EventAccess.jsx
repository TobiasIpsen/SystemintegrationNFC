import React, { useEffect, useRef, useState } from "react";
import {
  Box,
  Paper,
  Typography,
  Grid,
  TextField,
  MenuItem,
  Button,
  Avatar,
  Chip,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Stack,
} from "@mui/material";
import { alpha } from "@mui/material/styles";
import NfcIcon from "@mui/icons-material/Nfc";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import CancelIcon from "@mui/icons-material/Cancel";
import HistoryOutlinedIcon from "@mui/icons-material/HistoryOutlined";
import Inventory2OutlinedIcon from "@mui/icons-material/Inventory2Outlined";
import SectionHeader from "./SectionHeader.jsx";
import EmptyState from "./EmptyState.jsx";
import * as api from "../api.js";

const STATUS = {
  allowed: {
    color: "success.main",
    hex: "#1E9E5A",
    label: "Allowed",
    Icon: CheckCircleIcon,
  },
  notJoined: {
    color: "warning.main",
    hex: "#E08A1E",
    label: "Not Created / Not Joined This Event",
    Icon: WarningAmberIcon,
  },
  already: {
    color: "error.main",
    hex: "#D64550",
    label: "Already Accessed This Event",
    Icon: CancelIcon,
  },
};

export default function EventAccess() {
  const [events, setEvents] = useState([]);
  const [eventId, setEventId] = useState("");
  const [scan, setScan] = useState(null);
  const [history, setHistory] = useState([]);

  useEffect(() => {
    if (!eventId) {
      setHistory([]);
      return;
    }
    api
      .fetchAccessLog(eventId)
      .then((data) => {
        // Handle both student objects and student IDs for backward compatibility
        const studentList = Array.isArray(data)
          ? data.map((item) => (typeof item === "object" ? item : item))
          : [];
        setHistory(studentList.filter(Boolean));
      })
      .catch(() => setHistory([]));
  }, [eventId]);

  const current = scan ? STATUS[scan.status] : null;

  /* ---------------------------------------------------- */
  /* ---------------------------------------------------- */
  /* ---------------------WEBSOCKETS--------------------- */
  /* ---------------------------------------------------- */
  /* ---------------------------------------------------- */

  const [clientId] = useState(() => {
    const stored = sessionStorage.getItem("clientId");
    if (stored) return stored;

    const newId = crypto.randomUUID();
    sessionStorage.setItem("clientId", newId);
    return newId;
  });

  const [scanners, setScanners] = useState([]);
  const [selectedScannerId, setSelectedScannerId] = useState(null);
  const [message, setMessage] = useState([]);
  const [wsConnected, setWsConnected] = useState(false);
  const wsRef = useRef(null);

  useEffect(() => {
    if (wsRef.current) return;

    const clientIdValue = clientId;
    const ws = new WebSocket(
      `ws://localhost:5182/ws?clientId=${clientIdValue}`,
    );

    ws.onopen = () => {
      console.log("✓ WebSocket connected");
      setWsConnected(true);
      wsRef.current = ws;
    };

    ws.onmessage = (event) => {
      const data = JSON.parse(event.data);

      if (data.type === "scanner_list_updated") {
        console.log("[SCANNERS]", data.scanners);
        setScanners(data.scanners);
      } else if (data.type === "event_list_updated") {
        console.log("[EVENTS]", data.events);
        setEvents(data.events);
        if (!eventId && data.events.length > 0) {
          setEventId(data.events[0].Id);
        }
      } else if (data.type === "scanner_selected") {
        console.log("[SELECTED SCANNER]", data.scannerId);
        setSelectedScannerId(data.scannerId);
      } else if (data.type === "scanner_message") {
        const result = data.data;

        if (result.status) {
          const { student, status } = result;

          if (status === "allowed") {
            setHistory((prev) => [...prev, student]);
          }

          console.log(result);
          console.log("Card Id received:", result.student?.CardId);
          setScan({ student, status });
        }
      }
    };

    ws.onerror = (err) => {
      console.log("WebSocket error:", err);
      setWsConnected(false);
    };

    ws.onclose = () => {
      console.log("✗ WebSocket disconnected");
      setWsConnected(false);
      wsRef.current = null;
      setSelectedScannerId("");
      setScanners([]);
      setEvents([]);
      setEventId("");
    };

    return () => {
      if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
        wsRef.current.close();
      }
    };
  }, []);

  const handleSelectScanner = (event) => {
    const scannerId = event.target.value;
    if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
      console.log(`Selecting scanner: ${scannerId}`);
      const data = JSON.stringify({
        type: "select_scanner",
        scannerId: scannerId,
      });
      wsRef.current.send(data);
    } else {
      console.warn("WebSocket not connected");
    }
  };

  const handleSelectEvent = (eventId) => {  
    if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
      console.log(`Selecting event: ${eventId}`);
      const data = JSON.stringify({
        type: "select_event",
        eventId: eventId,
      });
      wsRef.current.send(data);
    } else {
      console.warn("WebSocket not connected");
    }
  };

  /* ---------------------------------------------------- */
  /* ---------------------------------------------------- */

  return (
    <Box
      sx={{
        minHeight: "78vh",
        borderRadius: 4,
        p: { xs: 2.5, sm: 4 },
        transition: "background 0.35s ease",
        background: current
          ? `linear-gradient(180deg, ${alpha(current.hex, 0.16)} 0%, ${alpha(current.hex, 0.04)} 45%)`
          : "linear-gradient(180deg, #EEF0FB 0%, #F5F6FB 45%)",
      }}
    >
      <Paper sx={{ p: 3, maxWidth: 720, mx: "auto", mb: 3 }}>
        <Stack
          direction="row"
          spacing={1.2}
          alignItems="center"
          sx={{ mb: 2.5 }}
        >
          <Box
            sx={{
              width: 36,
              height: 36,
              borderRadius: 2.5,
              display: "grid",
              placeItems: "center",
              bgcolor: alpha("#33307A", 0.1),
              color: "primary.main",
            }}
          >
            <NfcIcon fontSize="small" />
          </Box>
          <Typography variant="h6">Scan a Card</Typography>
        </Stack>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} sm={6}>
            <TextField
              select
              fullWidth
              label="Event"
              value={eventId ?? ""}
              onChange={(e) => {
                setEventId(e.target.value);
                setScan(null);
                handleSelectEvent(e.target.value);
              }}
            >
              {events.length === 0 && (
                <MenuItem value="" disabled>
                  No events yet
                </MenuItem>
              )}
              {events.map((ev) => (
                <MenuItem key={ev.Id} value={ev.Id}>
                  {ev.Name}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              select
              fullWidth
              label="Scanner"
              value={selectedScannerId ?? ""}
              onChange={handleSelectScanner}
            >
              {scanners.length === 0 && (
                <MenuItem value={null} disabled>
                  No scanners yet
                </MenuItem>
              )}
              {scanners.map((scanner) => (
                <MenuItem key={scanner.id} value={scanner.id}>
                  {scanner.id}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
        </Grid>
      </Paper>

      {scan && (
        <Paper
          elevation={0}
          sx={{
            p: 4,
            maxWidth: 720,
            mx: "auto",
            mb: 3,
            textAlign: "center",
            border: "2px solid",
            borderColor: current.color,
            bgcolor: "background.paper",
          }}
        >
          <Avatar
            src={scan.student?.Image}
            sx={{
              width: 104,
              height: 104,
              mx: "auto",
              mb: 2,
              fontSize: 36,
              border: "4px solid",
              borderColor: current.color,
            }}
          >
            {scan.student?.Name?.[0] || "?"}
          </Avatar>
          <Typography variant="h5">
            {scan.student?.Name || "Unknown Card"}
          </Typography>
          {scan.student?.ClassName && (
            <Typography variant="body2" color="text.secondary">
              {scan.student.ClassName}
            </Typography>
          )}
          <Chip
            sx={{ mt: 2, px: 1, fontSize: "0.85rem", height: 34 }}
            icon={<current.Icon />}
            label={current.label}
            color={
              scan.status === "allowed"
                ? "success"
                : scan.status === "already"
                  ? "error"
                  : "warning"
            }
          />
        </Paper>
      )}

      <Paper sx={{ p: { xs: 3, sm: 4 }, maxWidth: 720, mx: "auto" }}>
        <SectionHeader
          icon={<HistoryOutlinedIcon fontSize="small" />}
          title={`History (${history.length})`}
        />
        {history.length === 0 ? (
          <EmptyState
            icon={
              <Inventory2OutlinedIcon sx={{ fontSize: 36, opacity: 0.4 }} />
            }
            text="No one has scanned in for this event yet."
          />
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Image</TableCell>
                <TableCell>Name</TableCell>
                <TableCell>Class</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {history.map((student) => {
                return (
                  <TableRow key={student.Id} hover>
                    <TableCell>
                      <Avatar
                        src={student.Image}
                        sx={{ width: 32, height: 32 }}
                      >
                        {student.Name[0]}
                      </Avatar>
                    </TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>
                      {student.Name}
                    </TableCell>
                    <TableCell>{student.ClassName}</TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        )}
      </Paper>
    </Box>
  );
}
