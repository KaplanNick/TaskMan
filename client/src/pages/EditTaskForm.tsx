import { useState, useEffect, useRef } from 'react';
import { useUpdateTaskMutation, useGetTaskByIdQuery } from '../services/tasksApi';
import { useGetAllTagsQuery } from '../services/tagsApi';
import { validateTask, type TaskValidationErrors } from '../validation/taskValidation';
import { getErrorMessage } from '../types/api';
import {
  TextField,
  Button,
  Box,
  Typography,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Chip,
  OutlinedInput,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 250,
    },
  },
};

interface EditTaskFormProps {
  taskId: number;
  onSuccess?: () => void;
}

export function EditTaskForm({ taskId, onSuccess }: EditTaskFormProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [priority, setPriority] = useState<number>(1);
  const [selectedTags, setSelectedTags] = useState<number[]>([]);
  const [errors, setErrors] = useState<TaskValidationErrors>({});

  // Keep callback ref stable to avoid useEffect dependency issues
  const onSuccessRef = useRef(onSuccess);
  
  useEffect(() => {
    onSuccessRef.current = onSuccess;
  }, [onSuccess]);

  const [updateTask, { isLoading, isSuccess, isError, error }] = useUpdateTaskMutation();
  const { data: task, isLoading: isLoadingTask } = useGetTaskByIdQuery(taskId);
  const { data: tags } = useGetAllTagsQuery();

  // Pre-populate form when task data loads
  useEffect(() => {
    if (task) {
      setTitle(task.title);
      setDescription(task.description);
      setDueDate(task.dueDate.split('T')[0]); // Format for date input
      setPriority(task.priority);
      setSelectedTags(task.tags.map(tag => tag.id));
    }
  }, [task]);

  // Call onSuccess when update succeeds
  // Use only isSuccess in dependency array - callback is accessed via ref
  useEffect(() => {
    if (isSuccess && onSuccessRef.current) {
      onSuccessRef.current();
    }
  }, [isSuccess]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Task is guaranteed to exist at this point due to guard checks
    if (!task) {
      setErrors({ title: 'Task data not loaded' });
      return;
    }

    const validationErrors = validateTask(
      title,
      description,
      dueDate,
      priority,
      task.userId, // Use existing userId - task guaranteed to exist
      selectedTags,
      '', // No new user
      '',
      ''
    );
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});

    try {
      await updateTask({
        id: taskId,
        body: {
          title,
          description,
          dueDate,
          priority,
          tagIds: selectedTags,
        },
      }).unwrap();
    } catch (err) {
      // Error handled by RTK Query
    }
  };

  const handleTagChange = (event: SelectChangeEvent<number[]>) => {
    const {
      target: { value },
    } = event;
    setSelectedTags(typeof value === 'string' ? value.split(',').map(Number) : value);
  };

  const handleClear = () => {
    if (task) {
      setTitle(task.title);
      setDescription(task.description);
      setDueDate(task.dueDate.split('T')[0]);
      setPriority(task.priority);
      setSelectedTags(task.tags.map(tag => tag.id));
      setErrors({});
    }
  };

  if (isLoadingTask) {
    return <Typography>Loading task...</Typography>;
  }

  if (!task) {
    return <Typography>Task not found</Typography>;
  }

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ maxWidth: 600, mx: 'auto', mt: 4 }}>
      <Typography variant="h5" component="h1" gutterBottom>
        Edit Task
      </Typography>

      {isSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Task updated successfully!
        </Alert>
      )}

      {isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to update task: {getErrorMessage(error)}
        </Alert>
      )}

      <TextField
        fullWidth
        label="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        error={!!errors.title}
        helperText={errors.title || `${title.trim().length}/200 characters (min 3)`}
        margin="normal"
        required
        disabled={isLoading}
        slotProps={{ htmlInput: { maxLength: 200 } }}
      />

      <TextField
        fullWidth
        label="Description"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        error={!!errors.description}
        helperText={errors.description || `${description.trim().length}/2000 characters (min 10)`}
        margin="normal"
        multiline
        rows={3}
        required
        disabled={isLoading}
        slotProps={{ htmlInput: { maxLength: 2000 } }}
      />

      <TextField
        fullWidth
        label="Due Date"
        type="date"
        value={dueDate}
        onChange={(e) => setDueDate(e.target.value)}
        error={!!errors.dueDate}
        helperText={errors.dueDate}
        margin="normal"
        required
        disabled={isLoading}
        InputLabelProps={{
          shrink: true,
        }}
      />

      <FormControl fullWidth margin="normal" error={!!errors.priority}>
        <InputLabel>Priority</InputLabel>
        <Select
          value={priority}
          label="Priority"
          onChange={(e) => setPriority(Number(e.target.value))}
          disabled={isLoading}
        >
          <MenuItem value={1}>Low</MenuItem>
          <MenuItem value={2}>Medium</MenuItem>
          <MenuItem value={3}>High</MenuItem>
        </Select>
        {errors.priority && <FormHelperText>{errors.priority}</FormHelperText>}
      </FormControl>

      <FormControl fullWidth margin="normal" error={!!errors.tags}>
        <InputLabel>Tags</InputLabel>
        <Select
          multiple
          value={selectedTags}
          onChange={handleTagChange}
          input={<OutlinedInput label="Tags" />}
          disabled={isLoading}
          renderValue={(selected) => (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
              {selected.map((tagId) => {
                const tag = tags?.find((t) => t.id === tagId);
                return <Chip key={tagId} label={tag?.name || tagId} />;
              })}
            </Box>
          )}
          MenuProps={MenuProps}
        >
          {tags?.map((tag) => (
            <MenuItem key={tag.id} value={tag.id}>
              {tag.name}
            </MenuItem>
          ))}
        </Select>
        {errors.tags && <FormHelperText>{errors.tags}</FormHelperText>}
      </FormControl>

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
          {isLoading ? 'Updating...' : 'Update'}
        </Button>
        <Button
          type="button"
          variant="outlined"
          onClick={handleClear}
          sx={{ flex: 1 }}
        >
          Reset
        </Button>
        <Button
          type="button"
          variant="text"
          onClick={() => onSuccess?.()}
          sx={{ flex: 1 }}
        >
          Cancel
        </Button>
      </Box>
    </Box>
  );
}