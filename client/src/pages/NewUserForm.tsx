import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateUserMutation, useGetAllUsersQuery } from '../services/usersApi';
import { validateUser, type UserValidationErrors } from '../validation/userValidation';
import { getErrorMessage } from '../types/api';
import {
  TextField,
  Button,
  Box,
  Typography,
  Alert,
  CircularProgress,
} from '@mui/material';

export function NewUserForm() {
  const navigate = useNavigate();
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [telephone, setTelephone] = useState('');
  const [errors, setErrors] = useState<UserValidationErrors>({});

  const [createUser, { isLoading, isSuccess, isError, error }] = useCreateUserMutation();
  const { data: existingUsers = [] } = useGetAllUsersQuery();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const validationErrors = validateUser(fullName, email, telephone);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    // Check for duplicate email or telephone before submitting
    const duplicateErrors: UserValidationErrors = {};
    
    const emailExists = existingUsers.some(
      user => user.email.toLowerCase() === email.toLowerCase()
    );
    if (emailExists) {
      duplicateErrors.email = `A user with the email '${email}' already exists.`;
    }

    const telephoneExists = existingUsers.some(
      user => user.telephone === telephone
    );
    if (telephoneExists) {
      duplicateErrors.telephone = `A user with the telephone '${telephone}' already exists.`;
    }

    if (Object.keys(duplicateErrors).length > 0) {
      setErrors(duplicateErrors);
      return;
    }

    setErrors({});

    try {
      await createUser({
        fullName,
        email,
        telephone,
      }).unwrap();

      // Reset form on success
      setFullName('');
      setEmail('');
      setTelephone('');

      // Navigate back to tasks
      navigate('/');
    } catch (err) {
      // Map error message to the correct field based on error content
      const errorMessage = getErrorMessage(err);
      const mappedErrors: any = {};

      // Check which field caused the error based on message content
      if (errorMessage.toLowerCase().includes('email')) {
        mappedErrors.email = errorMessage;
      } else if (errorMessage.toLowerCase().includes('telephone')) {
        mappedErrors.telephone = errorMessage;
      } else {
        // Generic error - show in full name field
        mappedErrors.fullName = errorMessage;
      }

      setErrors(prev => ({
        ...prev,
        ...mappedErrors
      }));
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
          Create New User
        </Typography>

      {isSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          User created successfully!
        </Alert>
      )}

      {isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to create user: {getErrorMessage(error)}
        </Alert>
      )}

      <TextField
        fullWidth
        label="Full Name"
        value={fullName}
        onChange={(e) => setFullName(e.target.value)}
        error={!!errors.fullName}
        helperText={errors.fullName}
        margin="normal"
        required
        disabled={isLoading}
      />

      <TextField
        fullWidth
        label="Email"
        type="email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        error={!!errors.email}
        helperText={errors.email}
        margin="normal"
        required
        disabled={isLoading}
      />

      <TextField
        fullWidth
        label="Telephone"
        value={telephone}
        onChange={(e) => setTelephone(e.target.value)}
        error={!!errors.telephone}
        helperText={errors.telephone}
        margin="normal"
        required
        disabled={isLoading}
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
          startIcon={isLoading ? <CircularProgress size={20} color="inherit" /> : null}
        >
          {isLoading ? 'Creating...' : 'Create User'}
        </Button>
        <Button
          type="button"
          variant="outlined"
          onClick={() => {
            setFullName('');
            setEmail('');
            setTelephone('');
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
