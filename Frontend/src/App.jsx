import React, { useEffect, useRef, useState } from "react";
import { alpha } from "@mui/material/styles";
import {
  AppBar,
  Toolbar,
  Typography,
  Tabs,
  Tab,
  Box,
  Container,
  Stack,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from "@mui/material";
import ShieldOutlinedIcon from "@mui/icons-material/ShieldOutlined";
import PersonAddAltIcon from "@mui/icons-material/PersonAddAlt";
import AdminPanelSettingsOutlinedIcon from "@mui/icons-material/AdminPanelSettingsOutlined";
import NfcIcon from "@mui/icons-material/Nfc";

import UserPanel from "./components/UserPanel.jsx";
import AdminPanel from "./components/AdminPanel.jsx";
import EventAccess from "./components/EventAccess.jsx";

import * as api from "./api.js";

// ---------- App ----------
const TAB_META = [
  { label: "User Panel", icon: <PersonAddAltIcon /> },
  { label: "Admin Panel", icon: <AdminPanelSettingsOutlinedIcon /> },
  { label: "Event Access", icon: <NfcIcon /> },
];

export default function App() {
  const [tab, setTab] = useState(0);
  const [students, setStudents] = useState([]);
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [connectionError, setConnectionError] = useState("");

  const refresh = async () => {
    const [s, e] = await Promise.all([api.fetchStudents(), api.fetchEvents()]);
    setStudents(s);
    setEvents(e);
    setConnectionError("");
  };

  useEffect(() => {
    refresh()
      .catch((err) =>
        setConnectionError(err.message || "Could not reach the API server."),
      )
      .finally(() => setLoading(false));
  }, []);

  // const [clientId] = useState(() => {
  //   const stored = sessionStorage.getItem("clientId");
  //   if (stored) return stored;

  //   const newId = crypto.randomUUID();
  //   sessionStorage.setItem("clientId", newId);
  //   return newId;
  // });

  // const [scanners, setScanners] = useState([]);
  // const [selectedScannerId, setSelectedScannerId] = useState(null);
  // const [message, setMessage] = useState([]);
  // const [wsConnected, setWsConnected] = useState(false);
  // const wsRef = useRef(null);

  // useEffect(() => {
  //   if (wsRef.current) return;

  //   const clientIdValue = clientId;
  //   const ws = new WebSocket(
  //     `ws://localhost:5182/ws?clientId=${clientIdValue}`,
  //   );

  //   ws.onopen = () => {
  //     console.log("✓ WebSocket connected");
  //     setWsConnected(true);
  //     wsRef.current = ws;
  //   };

  //   ws.onmessage = (event) => {
  //     const data = JSON.parse(event.data);

  //     if (data.type === "scanner_list_updated") {
  //       console.log(data.scanners);
  //       setScanners(data.scanners);
  //     } else if (data.type === "scanner_selected") {
  //       console.log(data.scannerId);
  //       setSelectedScannerId(data.scannerId);
  //     } else if (data.type === "scanner_message") {
  //       console.log(data.data);
  //       setMessage((prev) => [
  //         ...prev,
  //         {
  //           from: data.scannerId,
  //           data: data.data,
  //           timestamp: new Date(data.timestamp),
  //         },
  //       ]);
  //     }
  //   };

  //   ws.onerror = (err) => {
  //     console.log("WebSocket error:", err);
  //     setWsConnected(false);
  //   };

  //   ws.onclose = () => {
  //     console.log("✗ WebSocket disconnected");
  //     setWsConnected(false);
  //     wsRef.current = null;
  //     setSelectedScannerId(null);
  //     setScanners([])
  //   };

  //   return () => {
  //     if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
  //       wsRef.current.close();
  //     }
  //   };
  // }, []);

  // const handleSelectScanner = (event) => {
  //   const scannerId = event.target.value;
  //   if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
  //     console.log(`Selecting scanner: ${scannerId}`);
  //     const data = JSON.stringify({
  //         type: "select_scanner",
  //         scannerId: scannerId,
  //       });
  //     wsRef.current.send(
  //       data
  //     );
  //   } else {
  //     console.warn("WebSocket not connected");
  //   }
  // };

  return (
    <Box>
      <AppBar
        position="static"
        elevation={0}
        sx={{
          background:
            "linear-gradient(120deg, #221F5C 0%, #33307A 60%, #453F9E 100%)",
        }}
      >
        <Toolbar sx={{ py: 1.5 }}>
          <Stack
            direction="row"
            spacing={1.5}
            alignItems="center"
            sx={{ flexGrow: 1 }}
          >
            <Box
              sx={{
                width: 40,
                height: 40,
                borderRadius: 2.5,
                bgcolor: alpha("#fff", 0.15),
                display: "grid",
                placeItems: "center",
              }}
            >
              <ShieldOutlinedIcon />
            </Box>
            <Box>
              <Typography variant="h6" sx={{ lineHeight: 1.1 }}>
                Campus Access
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>
                Access control, made simple
              </Typography>
            </Box>
          </Stack>
        </Toolbar>
        <Tabs
          value={tab}
          onChange={(_, v) => setTab(v)}
          textColor="inherit"
          TabIndicatorProps={{
            style: { height: 3, borderRadius: 3, backgroundColor: "#17B896" },
          }}
        >
          {TAB_META.map((t) => (
            <Tab
              key={t.label}
              icon={t.icon}
              iconPosition="start"
              label={t.label}
              sx={{ minHeight: 52, opacity: 0.85 }}
            />
          ))}
        </Tabs>
      </AppBar>
      {/* <FormControl variant="outlined" sx={{ width: 300 }}>
        <InputLabel id="selectScannerLabel">Select Scanner {wsConnected ? "✓" : "✗"}</InputLabel>
        <Select
          labelId="selectScannerLabel"
          id="selectScanner"
          value={selectedScannerId}
          label={`Select Scanner ${wsConnected ? "✓" : "✗"}`}
          onChange={handleSelectScanner}
        >
          <MenuItem value={null}>None</MenuItem>
          {scanners.map((data) => (
            <MenuItem key={data.id} value={data.id}>
              {data.id}
            </MenuItem>
          ))}
        </Select>
      </FormControl> */}

      <Container maxWidth="lg" sx={{ py: 5 }}>
        {connectionError && (
          <Alert severity="error" sx={{ mb: 3, maxWidth: 920, mx: "auto" }}>
            {connectionError} — make sure <code>docker compose up</code> and the{" "}
            <code>server</code> (npm run dev) are both running.
          </Alert>
        )}
        {loading ? (
          <Stack alignItems="center" sx={{ py: 10 }}>
            <CircularProgress />
          </Stack>
        ) : (
          <>
            {tab === 0 && <UserPanel events={events} refresh={refresh} />}
            {tab === 1 && (<AdminPanel students={students} events={events} onRefresh={refresh} />)}
            {tab === 2 && <EventAccess />}
          </>
        )}
      </Container>
    </Box>
  );
}
