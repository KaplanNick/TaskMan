import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateTagMutation, useGetAllTagsQuery } from '../services/tagsApi';
import { validateTag, type TagValidationErrors } from '../validation/tagValidation';
import { getErrorMessage } from '../types/api';
import {
  TextField,
  Button,
  Box,
  Typography,
  Alert,
} from '@mui/material';

export function NewTagForm() {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [errors, setErrors] = useState<TagValidationErrors>({});

  const [createTag, { isLoading, isSuccess, isError, error }] = useCreateTagMutation();
  const { data: tags = [] } = useGetAllTagsQuery();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const validationErrors = validateTag(name, tags);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});

    try {
      await createTag({
        name,
      }).unwrap();

      // Reset form on success
      setName('');

      // Navigate back to tasks
      navigate('/');
    } catch (err) {
      // Error is handled by RTK Query
    }
  };

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Box
        component="form"
        onSubmit={handleSubmit}
        sx={{
          maxWidth: { xs: '100%', sm: 400 },
          width: '100%',
          p: { xs: 2, sm: 3 },
        }}
      >
        <Typography variant="h5" component="h1" gutterBottom>
          Create New Tag
        </Typography>

      {isSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Tag created successfully!
        </Alert>
      )}

      {isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to create tag: {getErrorMessage(error)}
        </Alert>
      )}

      <TextField
        fullWidth
        label="Name"
        value={name}
        onChange={(e) => setName(e.target.value)}
        error={!!errors.name}
        helperText={errors.name || `${name.trim().length}/50 characters (min 2)`}
        margin="normal"
        required
        slotProps={{ htmlInput: { maxLength: 50 } }}
      />

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
        >
          {isLoading ? 'Creating...' : 'Create Tag'}
        </Button>
        <Button
          type="button"
          variant="outlined"
          onClick={() => {
            setName('');
            setErrors({});
          }}
          sx={{ flex: 1 }}
        >
          Clear
        </Button>
        <Button
          type="button"
          variant="text"
          onClick={() => navigate('/')}
          sx={{ flex: 1 }}
        >
          Cancel
        </Button>
      </Box>
      </Box>
    </Box>
  );
}
