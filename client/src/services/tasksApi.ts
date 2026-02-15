import { baseApi } from "./baseApi";
import type {
  TaskDto,
  CreateTaskDto,
  UpdateTaskDto,
} from "../types/dtos";

export const tasksApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getAllTasks: builder.query<TaskDto[], void>({
      query: () => "/tasks",
      providesTags: ["Task"],
    }),

    getTaskById: builder.query<TaskDto, number>({
      query: (id) => `/tasks/${id}`,
      // @ts-ignore - unused params required by RTK Query signature
      providesTags: (_, __, id) => [{ type: "Task", id }],
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
      // @ts-ignore - unused params required by RTK Query signature
      invalidatesTags: (_, __, { id }) => [{ type: "Task", id }, "Task"],
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