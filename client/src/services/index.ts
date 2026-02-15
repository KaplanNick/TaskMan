/**
 * Central export for all API services
 */

export { baseApi } from "./baseApi";
export { tagsApi } from "./tagsApi";
export { usersApi } from "./usersApi";
export { tasksApi } from "./tasksApi";

// Re-export hooks for convenience
export {
  useGetAllTagsQuery,
  useGetTagByIdQuery,
  useCreateTagMutation,
  useUpdateTagMutation,
  useDeleteTagMutation,
} from "./tagsApi";

export {
  useGetAllUsersQuery,
  useGetUserByIdQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useDeleteUserMutation,
} from "./usersApi";

export {
  useGetAllTasksQuery,
  useGetTaskByIdQuery,
  useCreateTaskMutation,
  useUpdateTaskMutation,
  useDeleteTaskMutation,
} from "./tasksApi";
