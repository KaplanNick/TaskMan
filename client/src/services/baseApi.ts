import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";

/**
 * Base API configuration shared across all API services
 * Provides consistent base URL and error handling
 */
export const baseApi = createApi({
  reducerPath: "api",
  baseQuery: fetchBaseQuery({
    baseUrl: "http://localhost:5000/api",
    prepareHeaders: (headers) => {
      headers.set("Content-Type", "application/json");
      return headers;
    },
  }),
  tagTypes: ["Tag", "User", "Task"],
  endpoints: () => ({}),
});
