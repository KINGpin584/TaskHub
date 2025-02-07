import React, { useState, useEffect } from "react";
import axios from "axios";

const AddCategoryModal = ({ onClose }) => {
  const [name, setName] = useState("");
  // Set default priority to 1 (Low)
  const [priority, setPriority] = useState(1);
  const [categories, setCategories] = useState([]);

  // Fetch existing categories so that the list can be updated (if needed)
  useEffect(() => {
    axios
      .get("http://localhost:5087/api/categories")
      .then((res) => setCategories(res.data))
      .catch((err) => console.error("Error fetching categories:", err));
  }, []);

  // Handle submission for adding a new category.
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post("http://localhost:5087/api/categories", {
        name,
        priority
      });
      setCategories([...categories, response.data]); // update category list, if needed.
      alert("Category added successfully!");
      onClose();
    } catch (err) {
      console.error("Error adding category:", err);
      alert("Failed to add category");
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center">
      <div className="bg-white p-6 rounded shadow-lg w-96">
        <h2 className="text-xl font-bold mb-4">Add Category</h2>
        <form onSubmit={handleSubmit} className="space-y-3">
          {/* Category Name Input */}
          <input
            type="text"
            placeholder="Category Name"
            className="w-full p-2 border rounded"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
          {/* Priority Select Input */}
          <div>
            <label className="block text-sm font-medium mb-1">Priority</label>
            <select
              value={priority}
              onChange={(e) => setPriority(parseInt(e.target.value))}
              className="w-full p-2 border rounded"
            >
              <option value={1}>1 - Low</option>
              <option value={2}>2 - Medium</option>
              <option value={3}>3 - High</option>
              <option value={4}>4 - Critical</option>
            </select>
          </div>
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
              Add Category
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default AddCategoryModal;
