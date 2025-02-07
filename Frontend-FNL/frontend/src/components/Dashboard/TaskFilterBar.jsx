import React, { useState } from "react";

const TaskFilterBar = ({ onFilterChange }) => {
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [sortOrder, setSortOrder] = useState("Descending");
  const [dueDateFrom, setDueDateFrom] = useState("");
  const [dueDateTo, setDueDateTo] = useState("");

  const updateFilters = (newFilters) => {
    onFilterChange({
      searchQuery,
      status: statusFilter,
      sortOrder,
      dueDateFrom,
      dueDateTo,
      ...newFilters,
    });
  };

  return (
    <div className="flex flex-col md:flex-row items-start md:items-center gap-4 mb-4">
      <input
        type="text"
        placeholder="Search tasks..."
        className="border border-gray-300 rounded p-2 w-full md:w-1/3"
        value={searchQuery}
        onChange={(e) => {
          setSearchQuery(e.target.value);
          updateFilters({ searchQuery: e.target.value });
        }}
      />
      <select
        value={statusFilter}
        onChange={(e) => {
          setStatusFilter(e.target.value);
          updateFilters({ status: e.target.value });
        }}
        className="border border-gray-300 rounded p-2"
      >
        <option value="All">All Status</option>
        <option value="Incomplete">Incomplete</option>
        <option value="InProgress">InProgress</option> {/* Fixed here */}
        <option value="Completed">Completed</option>
      </select>
      <select
        value={sortOrder}
        onChange={(e) => {
          setSortOrder(e.target.value);
          updateFilters({ sortOrder: e.target.value });
        }}
        className="border border-gray-300 rounded p-2"
      >
        <option value="Ascending">Priority: Ascending</option>
        <option value="Descending">Priority: Descending</option>
      </select>
      <input
        type="date"
        value={dueDateFrom}
        onChange={(e) => {
          setDueDateFrom(e.target.value);
          updateFilters({ dueDateFrom: e.target.value });
        }}
        className="border border-gray-300 rounded p-2"
        placeholder="Due Date From"
      />
      <input
        type="date"
        value={dueDateTo}
        onChange={(e) => {
          setDueDateTo(e.target.value);
          updateFilters({ dueDateTo: e.target.value });
        }}
        className="border border-gray-300 rounded p-2"
        placeholder="Due Date To"
      />
    </div>
  );
};

export default TaskFilterBar;
