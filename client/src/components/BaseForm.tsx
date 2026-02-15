import { Box, Typography } from '@mui/material';
import { ReactNode } from 'react';

interface BaseFormProps {
  title: string;
  children: ReactNode;
  onSubmit: (e: React.FormEvent<HTMLFormElement>) => void | Promise<void>;
  maxWidth?: number;
}

/**
 * Base form component providing consistent layout for all forms
 */
export function BaseForm({ title, children, onSubmit, maxWidth = 600 }: BaseFormProps) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Box
        component="form"
        onSubmit={onSubmit}
        sx={{
          maxWidth: { xs: '100%', sm: maxWidth },
          width: '100%',
          p: { xs: 2, sm: 3 },
        }}
      >
        <Typography variant="h5" component="h1" gutterBottom>
          {title}
        </Typography>
        {children}
      </Box>
    </Box>
  );
}
