import { Alert } from '@mui/material';

interface FormAlertsProps {
  isSuccess?: boolean;
  isError?: boolean;
  successMessage?: string;
  errorMessage?: string;
}

/**
 * Displays success and error alerts for forms
 */
export function FormAlerts({ 
  isSuccess, 
  isError, 
  successMessage = 'Operation successful!',
  errorMessage = 'An error occurred'
}: FormAlertsProps) {
  return (
    <>
      {isSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {successMessage}
        </Alert>
      )}

      {isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {errorMessage}
        </Alert>
      )}
    </>
  );
}
