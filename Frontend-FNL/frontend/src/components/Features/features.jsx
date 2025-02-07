import React from "react";
import { Swiper, SwiperSlide } from "swiper/react";
import { Navigation, Pagination, Autoplay } from "swiper/modules";
import { motion, useInView } from "framer-motion"; // Framer Motion imports
import "swiper/css";
import "swiper/css/navigation";
import "swiper/css/pagination";

const TaskManagerFeatures = () => {
  const features = [
    {
      id: 1,
      title: "Task Scheduling",
      description: "Efficiently plan and schedule your tasks with our intuitive calendar integration.",
      imgSrc: "/task-manage.png", // Replace with your image path
    },
    {
      id: 2,
      title: "Reminders",
      description: "Never miss a deadline with personalized notification reminders.",
      imgSrc: "/task-manage.png", // Replace with your image path
    },
    {
      id: 3,
      title: "Analytics",
      description: "Track your productivity with detailed analytics and progress reports.",
      imgSrc: "/task-manage.png", // Replace with your image path
    },
    {
      id: 4,
      title: "Collaboration Tools",
      description: "Collaborate seamlessly with your team using integrated communication tools.",
      imgSrc: "/task-manage.png", // Replace with your image path
    },
  ];

  const sectionRef = React.useRef(null);
  const isInView = useInView(sectionRef, { once: true });

  return (
    <motion.div
      ref={sectionRef}
      initial={{ opacity: 0, y: 200 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 1 }}
      className="bg-black text-white py-10 flex flex-col items-center"
    >
      <h1 className="text-4xl font-bold mb-12 text-center w-full flex justify-center">
        Task Manager Features
      </h1>

      <Swiper
        modules={[Navigation, Pagination, Autoplay]}
        navigation={{
          nextEl: ".swiper-button-next",
          prevEl: ".swiper-button-prev",
        }}
        pagination={{
          clickable: true,
          bulletClass: "swiper-pagination-bullet",
          bulletActiveClass: "swiper-pagination-bullet-active",
        }}
        autoplay={{ delay: 3000 }}
        loop={true}
        className="w-3/4 mx-auto"
      >
        {features.map((feature) => (
          <SwiperSlide key={feature.id} className="flex flex-col items-center">
            <img
              src={feature.imgSrc}
              alt={feature.title}
              className="w-full h-80 object-cover rounded-md mb-6"
            />
            <h2 className="text-3xl font-bold text-center mb-4">{feature.title}</h2>
            <p className="text-gray-300 text-center mb-12">{feature.description}</p>
          </SwiperSlide>
        ))}

        <div className="swiper-button-next text-gray-400"></div>
        <div className="swiper-button-prev text-gray-400"></div>
      </Swiper>

      <style>
        {`
          .swiper-pagination-bullet {
            background-color: gray;
            opacity: 0.5;
            margin-top: 20px; /* Adjust distance from content */
          }
          .swiper-pagination-bullet-active {
            background-color: white;
            opacity: 1;
          }
        `}
      </style>
    </motion.div>
  );
};

export default TaskManagerFeatures;
