import React, { useEffect, useState, useCallback, useMemo } from "react";
import { useRecoilValue } from "recoil";
import axios from "axios";
import { userState } from "../../recoil/atom";
import connection, { startConnection } from "../../services/signalr-connection";
import UpdateTaskModal from "./UpdateTaskModal"; // ensure correct import path
import TaskFilterBar from "./TaskFilterBar";

const MyTasks = () => {
  const user = useRecoilValue(userState);
  const [tasks, setTasks] = useState([]);
  const [openMenu, setOpenMenu] = useState(null);
  const [taskToUpdate, setTaskToUpdate] = useState(null);
  const [filters, setFilters] = useState({
    searchQuery: "",
    status: "All",
    sortOrder: "Descending",
    dueDateFrom: "",
    dueDateTo: ""
  });

  // Convert numeric status to friendly string.
  const getStatusString = (status) => {
    switch (status) {
      case 0: return "Incomplete";
      case 1: return "InProgress";
      case 2: return "Completed";
      default: return status;
    }
  };

  // Fetch tasks for current user.
  const fetchTasks = useCallback(async () => {
    if (!user || !user.id) {
      console.error("No user data found.");
      return;
    }
    try {
      const response = await axios.get(`http://localhost:5087/api/users/${user.id}`);
      const subscribedTasks = response.data.subscribedTasks || [];
      setTasks(subscribedTasks);
      subscribedTasks.forEach((task) => {
        connection.invoke("SubscribeToTask", task.id)
          .catch((err) => console.error(`Error subscribing to task ${task.id}:`, err));
      });
    } catch (error) {
      console.error("Error fetching my tasks:", error);
    }
  }, [user]);

  const handleUnsubscribe = async (taskId) => {
    if (!user) return;
    try {
      await axios.delete(`http://localhost:5087/api/tasks/${taskId}/unsubscribe/${user.id}`);
      alert("Unsubscribed from task successfully!");
      setTasks(prevTasks => prevTasks.filter(task => task.id !== taskId));
      setOpenMenu(null);
    } catch (err) {
      console.error("Error unsubscribing:", err.response?.data || err.message);
      alert("Failed to unsubscribe from task");
    }
  };

  const handleDeleteTask = async (taskId) => {
    try {
      await axios.delete(`http://localhost:5087/api/tasks/${taskId}`);
      alert("Task deleted successfully!");
      setTasks(prevTasks => prevTasks.filter(task => task.id !== taskId));
      setOpenMenu(null);
    } catch (error) {
      console.error("Error deleting task:", error.response?.data || error.message);
      alert("Failed to delete task");
    }
  };

  const handleUpdateTask = (task) => {
    setTaskToUpdate(task);
    setOpenMenu(null);
  };

  const toggleMenu = (taskId) => {
    setOpenMenu(prevId => (prevId === taskId ? null : taskId));
  };

  useEffect(() => {
    startConnection();
    const taskUpdatedHandler = (updatedTask) => {
      setTasks(prevTasks => {
        const exists = prevTasks.find(task => task.id === updatedTask.id);
        if (exists) {
          return prevTasks.map(task =>
            task.id === updatedTask.id ? updatedTask : task
          );
        }
        if (updatedTask.subscribers && updatedTask.subscribers.some(sub => sub.userId === user.id)) {
          return [...prevTasks, updatedTask];
        }
        return prevTasks;
      });
      alert(`Task "${updatedTask.title}" has been updated!`);
    };
    connection.on("TaskUpdated", taskUpdatedHandler);
    return () => {
      connection.off("TaskUpdated", taskUpdatedHandler);
    };
  }, [user]);

  useEffect(() => {
    fetchTasks();
  }, [fetchTasks]);

  // Filtering and sorting: if dueDateFrom and dueDateTo are equal, use exact match.
  const filteredTasks = useMemo(() => {
    let filtered = [...tasks];
    // Text search.
    if (filters.searchQuery) {
      const query = filters.searchQuery.toLowerCase();
      filtered = filtered.filter(task =>
        task.title.toLowerCase().includes(query) ||
        task.description.toLowerCase().includes(query)
      );
    }
    // Exact status match.
    if (filters.status !== "All") {
      filtered = filtered.filter(task => getStatusString(task.status) === filters.status);
    }
    // Due date filtering.
    if (filters.dueDateFrom && filters.dueDateTo && filters.dueDateFrom === filters.dueDateTo) {
      // Exact date match: compare ISO date parts.
      filtered = filtered.filter(task => {
        const taskDate = new Date(task.dueDate).toISOString().split("T")[0];
        return taskDate === filters.dueDateFrom;
      });
    } else {
      if (filters.dueDateFrom) {
        filtered = filtered.filter(task => new Date(task.dueDate) >= new Date(filters.dueDateFrom));
      }
      if (filters.dueDateTo) {
        filtered = filtered.filter(task => new Date(task.dueDate) <= new Date(filters.dueDateTo));
      }
    }
    // Sort by priority.
    filtered.sort((a, b) => {
      return filters.sortOrder === "Ascending" ? a.priority - b.priority : b.priority - a.priority;
    });
    return filtered;
  }, [tasks, filters]);

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">✅ My Tasks</h1>
      <TaskFilterBar onFilterChange={setFilters} />
      {filteredTasks.length === 0 ? (
        <p>No tasks subscribed.</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredTasks.map(task => (
            <div key={task.id} className="p-4 bg-white rounded shadow relative">
              <h2 className="text-lg font-semibold">{task.title}</h2>
              <p className="text-gray-600">{task.description}</p>
              <p className="text-sm text-gray-500">Priority: {task.priority}</p>
              <p className="text-sm text-gray-500">Due: {new Date(task.dueDate).toLocaleDateString()}</p>
              <p className="text-sm text-gray-500">Status: {getStatusString(task.status)}</p>
              <div className="absolute top-2 right-2">
                <button onClick={() => toggleMenu(task.id)} className="text-xl focus:outline-none">&#8942;</button>
                {openMenu === task.id && (
                  <div className="absolute right-0 mt-2 w-40 bg-white shadow-lg rounded z-10">
                    <ul className="py-1">
                      <li>
                        <button onClick={() => handleUnsubscribe(task.id)} className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                          Unsubscribe
                        </button>
                      </li>
                      {(task.status === 2 || getStatusString(task.status) === "Completed") && (
                        <li>
                          <button onClick={() => handleDeleteTask(task.id)} className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                            Delete Task
                          </button>
                        </li>
                      )}
                      <li>
                        <button onClick={() => handleUpdateTask(task)} className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                          Update Task
                        </button>
                      </li>
                    </ul>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
      {taskToUpdate && (
        <UpdateTaskModal
          task={taskToUpdate}
          onClose={() => setTaskToUpdate(null)}
          onUpdateSuccess={fetchTasks}
        />
      )}
    </div>
  );
};

export default MyTasks;
