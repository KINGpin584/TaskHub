import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useRecoilValue } from "recoil";
import axios from "axios";
import { userState } from "../../recoil/atom";
import connection, { startConnection } from "../../services/signalr-connection";
import TaskFilterBar from "./TaskFilterBar";

const TaskHub = () => {
  const user = useRecoilValue(userState);
  const [tasks, setTasks] = useState([]);
  const [filters, setFilters] = useState({
    searchQuery: "",
    status: "All",
    sortOrder: "Descending",
    dueDateFrom: "",
    dueDateTo: ""
  });

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 6;

  const fetchTasks = useCallback(async () => {
    try {
      const response = await axios.get("http://localhost:5087/api/tasks/priority");
      setTasks(response.data);
    } catch (error) {
      console.error("Error fetching tasks:", error);
    }
  }, []);

  const handleSubscribe = async (taskId) => {
    try {
      await axios.post(`http://localhost:5087/api/tasks/${taskId}/subscribe/${user.id}`);
      alert("Subscribed to task successfully!");
      fetchTasks();
    } catch (error) {
      console.error("Error subscribing to task:", error.response?.data || error.message);
      alert("Failed to subscribe to task");
    }
  };

  useEffect(() => {
    startConnection();
    const taskUpdatedHandler = (updatedTask) => {
      setTasks(prevTasks =>
        prevTasks.map(task => task.id === updatedTask.id ? updatedTask : task)
      );
      alert(`Task "${updatedTask.title}" has been updated!`);
    };
    const taskCreatedHandler = (newTask) => {
      setTasks(prevTasks => {
        if (prevTasks.some(task => task.id === newTask.id)) return prevTasks;
        return [...prevTasks, newTask];
      });
      alert(`New task "${newTask.title}" has been created!`);
    };
    connection.on("TaskUpdated", taskUpdatedHandler);
    connection.on("TaskCreated", taskCreatedHandler);
    return () => {
      connection.off("TaskUpdated", taskUpdatedHandler);
      connection.off("TaskCreated", taskCreatedHandler);
    };
  }, []);

  useEffect(() => {
    fetchTasks();
  }, [fetchTasks]);

  // Helper: convert task status to friendly string.
  const getStatusString = (status) => {
    switch (status) {
      case 0:
        return "Incomplete";
      case 1:
        return "InProgress";
      case 2:
        return "Completed";
      default:
        return status;
    }
  };

  // Apply filtering.
  const filteredTasks = useMemo(() => {
    let filtered = [...tasks];
    if (filters.searchQuery) {
      const query = filters.searchQuery.toLowerCase();
      filtered = filtered.filter(
        (task) =>
          task.title.toLowerCase().includes(query) ||
          task.description.toLowerCase().includes(query)
      );
    }
    if (filters.status !== "All") {
      filtered = filtered.filter(
        (task) => getStatusString(task.status) === filters.status
      );
    }
    if (filters.dueDateFrom) {
      filtered = filtered.filter(
        (task) => new Date(task.dueDate) >= new Date(filters.dueDateFrom)
      );
    }
    if (filters.dueDateTo) {
      filtered = filtered.filter(
        (task) => new Date(task.dueDate) <= new Date(filters.dueDateTo)
      );
    }
    // Sort by priority.
    filtered.sort((a, b) => {
      if (filters.sortOrder === "Ascending") {
        return a.priority - b.priority;
      } else {
        return b.priority - a.priority;
      }
    });
    return filtered;
  }, [tasks, filters]);

  useEffect(() => {
    setCurrentPage(1);
  }, [filters]);

  const totalItems = filteredTasks.length;
  const totalPages = Math.ceil(totalItems / pageSize);
  const paginatedTasks = useMemo(() => {
    const startIndex = (currentPage - 1) * pageSize;
    return filteredTasks.slice(startIndex, startIndex + pageSize);
  }, [filteredTasks, currentPage, pageSize]);

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Task Hub</h1>
      <TaskFilterBar onFilterChange={setFilters} />
      {filteredTasks.length === 0 ? (
        <p>No tasks available.</p>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {paginatedTasks.map(task => (
              <div key={task.id} className="p-4 bg-white rounded shadow">
                <h2 className="text-lg font-semibold">{task.title}</h2>
                <p className="text-gray-600">{task.description}</p>
                <p className="text-sm text-gray-500">Priority: {task.priority}</p>
                <p className="text-sm text-gray-500">
                  Due: {new Date(task.dueDate).toLocaleDateString()}
                </p>
                <p className="text-sm text-gray-500">
                  Status: {getStatusString(task.status)}
                </p>
                {task.subscribers && task.subscribers.some(sub => sub.userId === user.id) ? (
                  <span className="text-green-600 font-semibold">Subscribed</span>
                ) : (
                  <button 
                    onClick={() => handleSubscribe(task.id)}
                    className="mt-2 px-4 py-2 bg-blue-600 text-white rounded"
                  >
                    Subscribe
                  </button>
                )}
              </div>
            ))}
          </div>
          <div className="flex justify-center items-center mt-4 space-x-2">
            <button 
              disabled={currentPage === 1}
              onClick={() => setCurrentPage(currentPage - 1)}
              className="px-3 py-1 border rounded disabled:opacity-50"
            >
              Prev
            </button>
            <span className="px-2">
              Page {currentPage} of {totalPages}
            </span>
            <button 
              disabled={currentPage === totalPages || totalPages === 0}
              onClick={() => setCurrentPage(currentPage + 1)}
              className="px-3 py-1 border rounded disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default TaskHub;
