import { createTheme, alpha } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#33307A', dark: '#221F5C', light: '#5F5AA8' },
    secondary: { main: '#17B896' },
    success: { main: '#1E9E5A' },
    warning: { main: '#E08A1E' },
    error: { main: '#D64550' },
    background: { default: '#F5F6FB', paper: '#FFFFFF' },
    text: { primary: '#1E2035', secondary: '#666A88' },
  },
  shape: { borderRadius: 14 },
  typography: {
    fontFamily: '"Plus Jakarta Sans", "Segoe UI", Roboto, sans-serif',
    h5: { fontWeight: 700 },
    h6: { fontWeight: 700 },
    subtitle1: { fontWeight: 600 },
    button: { fontWeight: 600, textTransform: 'none' },
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: { backgroundColor: '#F5F6FB' },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: { backgroundImage: 'none' },
        elevation1: { boxShadow: '0 2px 16px rgba(30, 32, 53, 0.06)' },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: { borderRadius: 10, paddingTop: 8, paddingBottom: 8 },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: { borderRadius: 8, fontWeight: 600 },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        head: {
          fontWeight: 700,
          fontSize: '0.72rem',
          letterSpacing: 0.4,
          color: '#666A88',
          backgroundColor: alpha('#33307A', 0.04),
          borderBottom: 'none',
        },
        root: { borderBottom: '1px solid #EEF0F7' },
      },
    },
    MuiTextField: {
      defaultProps: { size: 'small' },
    },
  },
});

export default theme;
