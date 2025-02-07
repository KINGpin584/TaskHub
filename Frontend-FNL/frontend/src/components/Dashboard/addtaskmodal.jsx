import React, { useState, useEffect } from "react";
import axios from "axios";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { useRecoilValue } from "recoil";
import { userState } from "../../recoil/atom";

const AddTaskModal = ({ onClose }) => {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [dueDate, setDueDate] = useState(new Date());
  const [priority, setPriority] = useState(1);
  const [categoryId, setCategoryId] = useState("");
  const [status, setStatus] = useState("Incomplete");
  const [categories, setCategories] = useState([]);
  const user = useRecoilValue(userState); // Get user object from Recoil state

  // Fetch Categories from API
  useEffect(() => {
    axios
      .get("http://localhost:5087/api/categories") // Update API URL
      .then((res) => setCategories(res.data))
      .catch((err) => console.error("Error fetching categories:", err));
  }, []);

  // Handle Form Submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    const taskData = {
      title,
      description,
      dueDate,
      priority,
      categoryId: parseInt(categoryId),
      userId: user?.id, // Retrieve user id from Recoil state
      status: status === "Incomplete" ? 0 : 1, // Enum Mapping
    };

    try {
      await axios.post("http://localhost:5087/api/tasks", taskData);
      alert("Task added successfully!");
      onClose();
    } catch (err) {
      console.error("Error adding task:", err);
      alert("Failed to add task");
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center">
      <div className="bg-white p-6 rounded shadow-lg w-96">
        <h2 className="text-xl font-bold mb-4">Add Task</h2>
        <form onSubmit={handleSubmit} className="space-y-3">
          {/* Title */}
          <input
            type="text"
            placeholder="Title"
            className="w-full p-2 border rounded"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
          />
          {/* Description */}
          <textarea
            placeholder="Description"
            className="w-full p-2 border rounded"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
          />
          {/* Due Date with Calendar & Clock */}
          <div>
            <label className="block text-sm font-medium">Due Date & Time</label>
            <DatePicker
              selected={dueDate}
              onChange={(date) => setDueDate(date)}
              showTimeSelect
              dateFormat="Pp"
              className="w-full p-2 border rounded"
            />
          </div>
          {/* Priority Selection */}
          <select
            className="w-full p-2 border rounded"
            value={priority}
            onChange={(e) => setPriority(e.target.value)}
          >
            <option value="1">Low</option>
            <option value="2">Medium</option>
            <option value="3">High</option>
            <option value="4">Urgent</option>
          </select>
          {/* Category Dropdown */}
          <select
            className="w-full p-2 border rounded"
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            required
          >
            <option value="">Select Category</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>
                {cat.name}
              </option>
            ))}
          </select>
          {/* Status Selection */}
          <select
            className="w-full p-2 border rounded"
            value={status}
            onChange={(e) => setStatus(e.target.value)}
          >
            <option value="Incomplete">Incomplete</option>
            <option value="Completed">Completed</option>
          </select>
          {/* Buttons */}
          <div className="flex justify-end space-x-2">
            <button
              type="button"
              className="px-4 py-2 bg-gray-300 rounded"
              onClick={onClose}
            >
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-blue-600 text-white rounded">
              Add Task
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default AddTaskModal;

