import React, { useState } from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import AddTaskModal from "./addtaskmodal";
import AddCategoryModal from "./addcategorymodal"; // Import AddCategoryModal component

const Dashboard = () => {
  const [showTaskModal, setShowTaskModal] = useState(false);
  const [showCategoryModal, setShowCategoryModal] = useState(false);
  const location = useLocation(); // Get current location

  return (
    <div className="flex h-screen mt-12">
      {/* Sidebar */}
      <div className="w-64 bg-gray-900 text-white p-5 flex flex-col h-full">
        <h2 className="text-xl font-bold mb-6">Task Manager</h2>
        <nav className="flex flex-col space-y-3">
          <Link to="/dashboard" className="py-2 px-4 bg-gray-800 hover:bg-gray-700 rounded">
            📋 Task Hub
          </Link>
          <Link to="/dashboard/my-tasks" className="py-2 px-4 bg-gray-800 hover:bg-gray-700 rounded">
            ✅ My Tasks
          </Link>
          <button
            onClick={() => setShowTaskModal(true)}
            className="py-2 px-4 bg-green-600 hover:bg-green-500 rounded text-center mt-4"
          >
            ➕ Add Task
          </button>
          <button
            onClick={() => setShowCategoryModal(true)}
            className="py-2 px-4 bg-blue-600 hover:bg-blue-500 rounded text-center mt-2"
          >
            📁 Add Category
          </button>
        </nav>
      </div>

      {/* Main Content */}
      <div className="flex-1 bg-gray-100 p-6">
        {showTaskModal && <AddTaskModal onClose={() => setShowTaskModal(false)} />}
        {showCategoryModal && <AddCategoryModal onClose={() => setShowCategoryModal(false)} />}
        <Outlet />
      </div>
    </div>
  );
};

export default Dashboard;