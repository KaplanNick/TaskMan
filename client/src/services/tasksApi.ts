import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";

export interface TagDto {
  id: number;
  name: string;
}

export interface TaskDto {
  id: number;
  title: string;
  description: string;
  dueDate: string;
  priority: number;
  userId: number;
  tags: TagDto[];
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: number;
  userId: number;
  tagIds: number[];
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: number;
  tagIds: number[];
}

export const tasksApi = createApi({
  reducerPath: "tasksApi",
  baseQuery: fetchBaseQuery({ baseUrl: "http://localhost:5000/api" }),
  tagTypes: ["Task"],
  endpoints: (builder) => ({
    getAllTasks: builder.query<TaskDto[], void>({
      query: () => "/tasks",
      providesTags: ["Task"],
    }),

    getTaskById: builder.query<TaskDto, number>({
      query: (id) => `/tasks/${id}`,
      providesTags: (result, error, id) => [{ type: "Task", id }],
    }),

    createTask: builder.mutation<TaskDto, CreateTaskDto>({
      query: (body) => ({
        url: "/tasks",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Task"],
    }),

    updateTask: builder.mutation<TaskDto, { id: number; body: UpdateTaskDto }>({
      query: ({ id, body }) => ({
        url: `/tasks/${id}`,
        method: "PUT",
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: "Task", id }, "Task"],
    }),

    deleteTask: builder.mutation<void, number>({
      query: (id) => ({
        url: `/tasks/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: ["Task"],
    }),
  }),
});

export const {
  useGetAllTasksQuery,
  useGetTaskByIdQuery,
  useCreateTaskMutation,
  useUpdateTaskMutation,
  useDeleteTaskMutation,
} = tasksApi;