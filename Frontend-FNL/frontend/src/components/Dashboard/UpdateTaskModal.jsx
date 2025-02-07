import React, { useEffect, useState } from "react";
import axios from "axios";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { useRecoilValue } from "recoil";
import { userState } from "../../recoil/atom";

const UpdateTaskModal = ({ task, onClose, onUpdateSuccess }) => {
  const [title, setTitle] = useState(task.title || "");
  const [description, setDescription] = useState(task.description || "");
  const [dueDate, setDueDate] = useState(task.dueDate ? new Date(task.dueDate) : new Date());
  const [priority, setPriority] = useState(
    task.priority !== undefined ? task.priority.toString() : ""
  );
  const [categoryId, setCategoryId] = useState(
    task.categoryId !== undefined ? task.categoryId.toString() : ""
  );
  const [status, setStatus] = useState(
    task.status === 0 ? "Incomplete" : task.status === 1 ? "InProgress" : "Completed"
  );
  const [categories, setCategories] = useState([]);
  
  useEffect(() => {
    axios
      .get("http://localhost:5087/api/categories")
      .then((res) => setCategories(res.data))
      .catch((err) => console.error("Error fetching categories:", err));
  }, []);
  
  const handleSubmit = async (e) => {
    e.preventDefault();
    let updateData = {};
    if (title.trim() !== "") updateData.Title = title.trim();
    if (description.trim() !== "") updateData.Description = description.trim();
    if (dueDate) updateData.DueDate = dueDate;
    if (priority && priority.trim() !== "") updateData.Priority = parseInt(priority);
    if (categoryId && categoryId.trim() !== "") updateData.CategoryId = parseInt(categoryId);
    if (status && status.trim() !== "")
      updateData.Status = status === "Incomplete" ? 0 : status === "InProgress" ? 1 : 2;
  
    try {
      await axios.put(`http://localhost:5087/api/tasks/${task.id}`, updateData);
      alert("Task updated successfully!");
      onUpdateSuccess();
      onClose();
    } catch (err) {
      console.error("Error updating task:", err.response?.data || err.message);
      alert("Failed to update task");
    }
  };
  
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50">
      <div className="bg-white p-6 rounded shadow-lg w-96">
        <h2 className="text-xl font-bold mb-4">Update Task</h2>
        <form onSubmit={handleSubmit} className="space-y-3">
          <input type="text" placeholder="Title" className="w-full p-2 border rounded" value={title} onChange={(e) => setTitle(e.target.value)} />
          <textarea placeholder="Description" className="w-full p-2 border rounded" value={description} onChange={(e) => setDescription(e.target.value)} />
          <div>
            <label className="block text-sm font-medium">Due Date & Time</label>
            <DatePicker selected={dueDate} onChange={(date) => setDueDate(date)} showTimeSelect dateFormat="Pp" className="w-full p-2 border rounded" />
          </div>
          <select className="w-full p-2 border rounded" value={priority} onChange={(e) => setPriority(e.target.value)}>
            <option value="">Select Priority</option>
            <option value="1">Low</option>
            <option value="2">Medium</option>
            <option value="3">High</option>
            <option value="4">Urgent</option>
            <option value="5">Very Urgent</option>
          </select>
          <select className="w-full p-2 border rounded" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
            <option value="">Select Category</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>{cat.name}</option>
            ))}
          </select>
          <select className="w-full p-2 border rounded" value={status} onChange={(e) => setStatus(e.target.value)}>
            <option value="Incomplete">Incomplete</option>
            <option value="InProgress">InProgress</option>
            <option value="Completed">Completed</option>
          </select>
          <div className="flex justify-end space-x-2">
            <button type="button" className="px-4 py-2 bg-gray-300 rounded" onClick={onClose}>Cancel</button>
            <button type="submit" className="px-4 py-2 bg-blue-600 text-white rounded">Update Task</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default UpdateTaskModal;
