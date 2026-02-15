import { Box, Button, CircularProgress } from '@mui/material';

interface FormActionsProps {
  isLoading?: boolean;
  submitLabel?: string;
  loadingLabel?: string;
  onClear?: () => void;
  onCancel?: () => void;
  clearLabel?: string;
  cancelLabel?: string;
  showClear?: boolean;
  showCancel?: boolean;
}

/**
 * Standardized form action buttons (Submit, Clear, Cancel)
 */
export function FormActions({
  isLoading = false,
  submitLabel = 'Submit',
  loadingLabel = 'Saving...',
  onClear,
  onCancel,
  clearLabel = 'Clear',
  cancelLabel = 'Cancel',
  showClear = true,
  showCancel = true,
}: FormActionsProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: { xs: 'column', sm: 'row' },
        gap: 2,
        mt: 3,
        mb: 2,
      }}
    >
      <Button
        type="submit"
        variant="contained"
        disabled={isLoading}
        sx={{ flex: 1 }}
        startIcon={isLoading ? <CircularProgress size={20} color="inherit" /> : null}
      >
        {isLoading ? loadingLabel : submitLabel}
      </Button>

      {showClear && onClear && (
        <Button
          type="button"
          variant="outlined"
          onClick={onClear}
          disabled={isLoading}
          sx={{ flex: 1 }}
        >
          {clearLabel}
        </Button>
      )}

      {showCancel && onCancel && (
        <Button
          type="button"
          variant="text"
          onClick={onCancel}
          disabled={isLoading}
          sx={{ flex: 1 }}
        >
          {cancelLabel}
        </Button>
      )}
    </Box>
  );
}
