-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Apr 27, 2025 at 06:40 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `nkdupvs_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `affair`
--

CREATE TABLE `affair` (
  `id_Affair` int(11) NOT NULL,
  `event_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `affair`
--

INSERT INTO `affair` (`id_Affair`, `event_id`) VALUES
(3, 8),
(5, 12),
(6, 13),
(7, 17),
(8, 19),
(9, 21),
(10, 24),
(11, 25),
(12, 26),
(13, 27),
(14, 28);

-- --------------------------------------------------------

--
-- Table structure for table `class`
--

CREATE TABLE `class` (
  `code` varchar(10) NOT NULL,
  `name` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `class`
--

INSERT INTO `class` (`code`, `name`) VALUES
('A123113', 'Atsibodo tas kalendorius jau'),
('CLS1740424', 'Nuoru valgyt'),
('CLS1740950', 'Katinui valgyt reikia įdėti'),
('CLS1741372', 'Pamiegot'),
('CLS1741384', 'Kaziuko mugė'),
('CLS1742330', 'Pietų miegelis 2'),
('CLS1742585', 'Atsibodo tas kalendorius jau'),
('CLS1743350', 'sdfdsfs'),
('CLS1744387', 'Miau miau miau'),
('CLS1744388', 'cvcvcv'),
('CLS8AKG364', 'Nueiti į maximą'),
('CLSAUIEU42', 'fgfg'),
('CLSG27SDFC', 'AAAAAAA'),
('CLSYD3J07K', 'Nueiti į rimi'),
('CLSOIB4VZ3', 'AAAAAAAAAAAA'),
('CLSVYL3BY3', 'B'),
('dddd', 'dsfsdf'),
('P000B112', 'Profesinė praktika'),
('T001B000', 'Pietų miegelis 0'),
('T001B002', 'Pietų miegelis 1'),
('T001B003', 'Pietų miegelis 3'),
('T001B004', 'Pietų miegelis 2');

-- --------------------------------------------------------

--
-- Table structure for table `classtypes`
--

CREATE TABLE `classtypes` (
  `id_ClassTypes` int(11) NOT NULL,
  `name` char(21) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `classtypes`
--

INSERT INTO `classtypes` (`id_ClassTypes`, `name`) VALUES
(1, 'laboratoriniai darbai'),
(2, 'pratybos'),
(3, 'seminaras'),
(4, 'teorija'),
(5, 'mentoriaus pridėtas');

-- --------------------------------------------------------

--
-- Table structure for table `departments`
--

CREATE TABLE `departments` (
  `id_Departments` int(11) NOT NULL,
  `name` char(24) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `departments`
--

INSERT INTO `departments` (`id_Departments`, `name`) VALUES
(1, 'Informacijos sistemų'),
(2, 'Kompiuterių'),
(3, 'Multimedijos inžinerijos'),
(4, 'Programų inžinerijos'),
(5, 'Taikomosios informatikos');

-- --------------------------------------------------------

--
-- Table structure for table `event`
--

CREATE TABLE `event` (
  `name` varchar(20) DEFAULT NULL,
  `startTime` datetime DEFAULT NULL,
  `endTime` datetime DEFAULT NULL,
  `address` varchar(50) DEFAULT NULL,
  `id_Event` int(11) NOT NULL,
  `comment` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `event`
--

INSERT INTO `event` (`name`, `startTime`, `endTime`, `address`, `id_Event`, `comment`) VALUES
('Nemušiu', '2025-04-09 12:45:00', '2025-04-09 13:30:00', 'Nuotoliniu būdu', 2, NULL),
('gfdgfdg', '2025-04-17 11:11:00', '2025-05-02 11:23:00', 'dfdfsdsf', 4, 'asdadasdasd'),
('Miau', '2025-04-09 16:00:00', '2025-04-10 14:30:00', 'cvbcvb', 5, NULL),
('sdfdsf', '2025-04-16 22:22:00', '2025-04-19 22:22:00', 'asdasdsad', 8, 'MIAU'),
('fdsfdsf', '2025-04-10 14:45:00', '2025-04-10 15:00:00', 'dsfdsf', 9, NULL),
('ffff', '2025-04-08 20:00:00', '2025-04-16 23:00:00', 'dsfdsf', 12, NULL),
('Miau miau', '2025-04-10 18:15:00', '2025-04-10 18:30:00', 'sd', 13, NULL),
('AAA', '2025-04-17 22:30:00', '2025-04-18 22:30:00', 'AAAAAA', 16, 'MIAAAAAAAAAAAAAAAUUUUUUUUUUU'),
('BBBBBBBBBBBBBBBB', '2025-04-19 22:30:00', '2025-04-20 22:30:00', 'BBBBBBBBBBB', 17, 'BBBBBBBBBBBBBBBBBBBBBBBBBBBBBbbb'),
('sadahjhj', '2025-04-16 01:00:00', '2025-04-16 01:30:00', 'sdsa', 18, 'asdasd'),
('sad', '2025-04-18 01:15:00', '2025-04-19 01:30:00', 'sa', 19, ''),
('pietukai', '2025-04-26 23:15:00', '2025-04-26 23:45:00', 'gf', 20, 'gf'),
('Miau miau miau', '2025-04-24 23:30:00', '2025-04-25 23:30:00', 'asd', 21, 'sad'),
('fgffd', '2025-04-27 23:00:00', '2025-04-28 23:00:00', 'dsdfsfds', 22, 'dsdsfdsf'),
('poguliukas', '2025-04-27 23:30:00', '2025-04-28 23:30:00', 'fdfds', 23, 'sddsf'),
('pamiegot', '2025-04-20 00:30:00', '2025-04-21 00:30:00', 'dsf', 24, 'dsf'),
('eisiu miegot tuoj', '2025-04-27 00:45:00', '2025-04-27 01:00:00', 'ggf', 25, 'ggf'),
('nnnnnnnnn', '2025-04-27 02:00:00', '2025-04-27 02:45:00', 'nnnnnnnnn', 26, 'nnnnnnnnnnnn'),
('nnnnnnnnn', '2025-04-27 02:00:00', '2025-04-27 02:45:00', 'nnnnnnnnn', 27, 'nnnnnnnnnnnn'),
('mmmmmmmmmmm', '2025-04-27 03:15:00', '2025-04-27 05:00:00', 'mmmmmmmmmm', 28, 'mmmmmmmmmmmmm');

-- --------------------------------------------------------

--
-- Table structure for table `feedback`
--

CREATE TABLE `feedback` (
  `comment` varchar(255) DEFAULT NULL,
  `submissionDate` date DEFAULT NULL,
  `rating` int(11) DEFAULT NULL,
  `id_Feedback` int(11) NOT NULL,
  `fk_Affairid_Affair` int(11) DEFAULT NULL,
  `fk_Trainingid_Training` int(11) DEFAULT NULL,
  `fk_SemesterPlanTask` int(11) DEFAULT NULL,
  `MenteeCode` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `feedback`
--

INSERT INTO `feedback` (`comment`, `submissionDate`, `rating`, `id_Feedback`, `fk_Affairid_Affair`, `fk_Trainingid_Training`, `fk_SemesterPlanTask`, `MenteeCode`) VALUES
('Puikūs bulviniai blynai.', '2025-03-30', 5, 7, NULL, NULL, 11, NULL),
('fdfgfdg', '2025-04-02', 4, 8, NULL, NULL, 18, NULL),
('Super', '2025-04-25', 5, 9, NULL, NULL, 31, NULL),
('Super duper', '2025-04-27', 5, 11, NULL, 10, NULL, 'E1089'),
('Kaip ir nieko', '2025-04-27', 4, 12, NULL, 10, NULL, 'G8144'),
('Super', '2025-04-27', 5, 13, 13, NULL, NULL, 'G8144'),
('asdasd', '2025-04-27', 5, 14, 9, NULL, NULL, 'E1089');

-- --------------------------------------------------------

--
-- Table structure for table `mentee`
--

CREATE TABLE `mentee` (
  `studyProgram` int(11) DEFAULT NULL,
  `code` varchar(8) NOT NULL,
  `specialization` int(11) DEFAULT NULL,
  `MentorCode` varchar(8) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `mentee`
--

INSERT INTO `mentee` (`studyProgram`, `code`, `specialization`, `MentorCode`) VALUES
(9, 'E1089', NULL, 'Z5506'),
(4, 'G8144', 2, 'Z5506'),
(4, 'L7127', 2, 'Z5506');

-- --------------------------------------------------------

--
-- Table structure for table `mentor`
--

CREATE TABLE `mentor` (
  `department` int(11) DEFAULT NULL,
  `code` varchar(8) NOT NULL,
  `AcceptingMentees` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `mentor`
--

INSERT INTO `mentor` (`department`, `code`, `AcceptingMentees`) VALUES
(1, 'Z5506', 1);

-- --------------------------------------------------------

--
-- Table structure for table `mentorexpertise`
--

CREATE TABLE `mentorexpertise` (
  `id` int(11) NOT NULL,
  `mentorCode` varchar(8) NOT NULL,
  `studyProgram` int(11) NOT NULL,
  `specialization` int(11) DEFAULT NULL,
  `priority` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `mentorexpertise`
--

INSERT INTO `mentorexpertise` (`id`, `mentorCode`, `studyProgram`, `specialization`, `priority`) VALUES
(48, 'Z5506', 4, 2, 1),
(49, 'Z5506', 4, 1, 2),
(50, 'Z5506', 9, NULL, 3);

-- --------------------------------------------------------

--
-- Table structure for table `mentorrequest`
--

CREATE TABLE `mentorrequest` (
  `id` int(11) NOT NULL,
  `menteeCode` varchar(8) NOT NULL,
  `mentorCode` varchar(8) NOT NULL,
  `requestDate` datetime NOT NULL DEFAULT current_timestamp(),
  `RequestStatusId` int(11) NOT NULL DEFAULT 1,
  `IsRead` tinyint(1) NOT NULL DEFAULT 0,
  `RejectionReason` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `mentorrequest`
--

INSERT INTO `mentorrequest` (`id`, `menteeCode`, `mentorCode`, `requestDate`, `RequestStatusId`, `IsRead`, `RejectionReason`) VALUES
(22, 'G8144', 'Z5506', '2025-03-13 22:30:14', 3, 1, 'Neaiškus tu kažkoks'),
(23, 'G8144', 'Z5506', '2025-03-13 22:31:52', 2, 1, NULL),
(24, 'E1089', 'Z5506', '2025-03-15 22:22:40', 2, 1, NULL),
(25, 'G8144', 'Z5506', '2025-03-19 09:11:21', 2, 1, NULL),
(26, 'G8144', 'Z5506', '2025-03-19 09:33:23', 3, 1, 'Nenoriu'),
(27, 'G8144', 'Z5506', '2025-03-19 09:34:08', 2, 1, NULL),
(28, 'G8144', 'Z5506', '2025-04-08 10:41:58', 3, 1, 'Nepatinki'),
(29, 'G8144', 'Z5506', '2025-04-08 10:44:40', 2, 1, NULL),
(30, 'L7127', 'Z5506', '2025-04-25 19:06:48', 2, 1, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `mentorsevent`
--

CREATE TABLE `mentorsevent` (
  `id_MentorEvent` int(11) NOT NULL,
  `fk_Eventid_Event` int(11) NOT NULL,
  `fk_Mentorcode` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

-- --------------------------------------------------------

--
-- Table structure for table `rejectionhistory`
--

CREATE TABLE `rejectionhistory` (
  `id` int(11) NOT NULL,
  `userCode` varchar(8) NOT NULL,
  `reason` varchar(255) NOT NULL,
  `rejectedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `rejectionhistory`
--

INSERT INTO `rejectionhistory` (`id`, `userCode`, `reason`, `rejectedAt`) VALUES
(17, 'Z5506', 'Ne ta katedra', '2025-03-07 20:35:51'),
(20, 'Z5506', 'Dar kartą reikia', '2025-03-07 20:51:28');

-- --------------------------------------------------------

--
-- Table structure for table `requeststatuses`
--

CREATE TABLE `requeststatuses` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `requeststatuses`
--

INSERT INTO `requeststatuses` (`Id`, `Name`) VALUES
(1, 'Laukiama patvirtinimo'),
(2, 'Priimta'),
(3, 'Atmesta');

-- --------------------------------------------------------

--
-- Table structure for table `semesterplan`
--

CREATE TABLE `semesterplan` (
  `id_SemesterPlan` int(11) NOT NULL,
  `semesterStartDate` date DEFAULT NULL,
  `semesterEndDate` date DEFAULT NULL,
  `fk_Mentorcode` varchar(8) NOT NULL,
  `fk_Menteecode` varchar(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `semesterplan`
--

INSERT INTO `semesterplan` (`id_SemesterPlan`, `semesterStartDate`, `semesterEndDate`, `fk_Mentorcode`, `fk_Menteecode`) VALUES
(1, '2025-02-01', '2025-05-31', 'Z5506', 'G8144'),
(2, '2025-02-01', '2025-05-31', 'Z5506', 'E1089');

-- --------------------------------------------------------

--
-- Table structure for table `semesterplanclass`
--

CREATE TABLE `semesterplanclass` (
  `id_SemesterPlanClass` int(11) NOT NULL,
  `fk_SemesterPlanid_SemesterPlan` int(11) NOT NULL,
  `fk_Classcode` varchar(10) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

-- --------------------------------------------------------

--
-- Table structure for table `semesterplanevent`
--

CREATE TABLE `semesterplanevent` (
  `id_SemesterPlanEvent` int(11) NOT NULL,
  `fk_SemesterPlanid_SemesterPlan` int(11) NOT NULL,
  `fk_Eventid_Event` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `semesterplanevent`
--

INSERT INTO `semesterplanevent` (`id_SemesterPlanEvent`, `fk_SemesterPlanid_SemesterPlan`, `fk_Eventid_Event`) VALUES
(1, 2, 16),
(2, 2, 4),
(3, 2, 8),
(4, 1, 16),
(5, 1, 18),
(6, 2, 17),
(7, 2, 21),
(8, 2, 24),
(9, 2, 25),
(10, 1, 25),
(11, 2, 26),
(12, 2, 27),
(13, 2, 28),
(14, 1, 22),
(15, 1, 27),
(16, 1, 26),
(17, 1, 28);

-- --------------------------------------------------------

--
-- Table structure for table `semesterplantask`
--

CREATE TABLE `semesterplantask` (
  `id_SemesterPlanTask` int(11) NOT NULL,
  `fk_SemesterPlanid_SemesterPlan` int(11) NOT NULL,
  `fk_Taskid_Task` int(11) NOT NULL,
  `completionFile` varchar(255) DEFAULT NULL,
  `isRated` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `semesterplantask`
--

INSERT INTO `semesterplantask` (`id_SemesterPlanTask`, `fk_SemesterPlanid_SemesterPlan`, `fk_Taskid_Task`, `completionFile`, `isRated`) VALUES
(11, 1, 13, 'https://docs.google.com/presentation/d/1rZ4jV69ARTYr2YoGm5683CzFppCK6-6onTqoZtLH0zE/edit?usp=sharing', 1),
(17, 2, 9, 'completed', 0),
(18, 1, 9, 'completed', 1),
(19, 2, 12, 'completed', 0),
(20, 1, 12, 'completed', 0),
(30, 1, 26, 'completed', 0),
(31, 2, 26, 'completed', 1);

-- --------------------------------------------------------

--
-- Table structure for table `specializations`
--

CREATE TABLE `specializations` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `fk_id_StudyPrograms` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `specializations`
--

INSERT INTO `specializations` (`id`, `name`, `fk_id_StudyPrograms`) VALUES
(1, 'Duomenų bazių valdymas ir programavimas', 4),
(2, 'Informacinių sistemų analizė ir projektavimas', 4),
(3, 'Interneto informatika', 5),
(4, 'Multimedijos sistemos', 5),
(5, 'Daiktų interneto technologijos', 6),
(6, 'Informacinės technologijos ir kibernetinis saugumas', 6),
(7, 'Skaitmeninio turinio inžinerija', 7),
(8, 'Žaidimų ir interaktyvių sistemų programavimas', 7);

-- --------------------------------------------------------

--
-- Table structure for table `studyprograms`
--

CREATE TABLE `studyprograms` (
  `id_StudyPrograms` int(11) NOT NULL,
  `name` char(47) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `studyprograms`
--

INSERT INTO `studyprograms` (`id_StudyPrograms`, `name`) VALUES
(1, 'Dirbtinio intelekto informatika'),
(2, 'Dirbtinis intelektas'),
(3, 'Informacijos ir informacinių technologijų sauga'),
(4, 'Informacinės sistemos'),
(5, 'Informatika'),
(6, 'Informatikos inžinerija'),
(7, 'Multimedijos technologijos'),
(8, 'Nuotolinio mokymosi informacinės technologijos'),
(9, 'Programų sistemos'),
(10, 'Programų sistemų inžinerija'),
(11, 'Veiklos skaitmeninimas ir sistemų architektūros');

-- --------------------------------------------------------

--
-- Table structure for table `task`
--

CREATE TABLE `task` (
  `name` varchar(50) DEFAULT NULL,
  `description` varchar(255) DEFAULT NULL,
  `materialLink` varchar(255) DEFAULT NULL,
  `deadline` datetime DEFAULT NULL,
  `isAssigned` tinyint(1) DEFAULT 0,
  `id_Task` int(11) NOT NULL,
  `createdBy` varchar(8) NOT NULL DEFAULT '',
  `taskTypeId` int(11) DEFAULT NULL,
  `fk_Eventid_Event` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `task`
--

INSERT INTO `task` (`name`, `description`, `materialLink`, `deadline`, `isAssigned`, `id_Task`, `createdBy`, `taskTypeId`, `fk_Eventid_Event`) VALUES
('Cepelinai', 'Išvirk', 'https://docs.google.com/presentation/d/1rZ4jV69ARTYr2YoGm5683CzFppCK6-6onTqoZtLH0zE/edit?usp=sharing', '2025-03-29 23:45:00', 1, 9, 'Z5506', 2, NULL),
('Kugeliss', 'Iškepk!', 'https://docs.google.com/presentation/d/1rZ4jV69ARTYr2YoGm5683CzFppCK6-6onTqoZtLH0zE/edit?usp=sharing', '2025-04-26 00:00:00', 1, 12, 'Z5506', 3, NULL),
('Bulviniai blynai', 'Iškepk', 'https://docs.google.com/presentation/d/1rZ4jV69ARTYr2YoGm5683CzFppCK6-6onTqoZtLH0zE/edit?usp=sharing', '2025-04-06 00:45:00', 1, 13, 'Z5506', 2, NULL),
('AAAAA', 'AAAAA', '', '2025-04-25 23:00:00', 1, 26, 'Z5506', 1, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `tasktypes`
--

CREATE TABLE `tasktypes` (
  `id` int(11) NOT NULL,
  `name` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

--
-- Dumping data for table `tasktypes`
--

INSERT INTO `tasktypes` (`id`, `name`) VALUES
(1, 'Renginys'),
(2, 'Dėl paskaitų vedimo'),
(3, 'Kita');

-- --------------------------------------------------------

--
-- Table structure for table `training`
--

CREATE TABLE `training` (
  `id_Training` int(11) NOT NULL,
  `event_id` int(11) DEFAULT NULL,
  `fk_ViceDeadForStudiescode` varchar(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `training`
--

INSERT INTO `training` (`id_Training`, `event_id`, `fk_ViceDeadForStudiescode`) VALUES
(2, 2, 'R6375'),
(4, 4, 'R6375'),
(5, 5, 'R6375'),
(6, 9, 'R6375'),
(10, 16, 'R6375'),
(11, 18, 'R6375'),
(12, 20, 'R6375'),
(13, 22, 'R6375'),
(14, 23, 'R6375');

-- --------------------------------------------------------

--
-- Table structure for table `user`
--

CREATE TABLE `user` (
  `username` varchar(10) DEFAULT NULL,
  `password` varchar(20) DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  `name` varchar(20) DEFAULT NULL,
  `lastName` varchar(30) DEFAULT NULL,
  `code` varchar(8) NOT NULL,
  `phoneNumber` varchar(15) DEFAULT NULL,
  `isAdmin` tinyint(1) DEFAULT 0,
  `isVerified` tinyint(1) DEFAULT 0,
  `isSubmitted` tinyint(1) DEFAULT 0,
  `isMentor` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `user`
--

INSERT INTO `user` (`username`, `password`, `email`, `name`, `lastName`, `code`, `phoneNumber`, `isAdmin`, `isVerified`, `isSubmitted`, `isMentor`) VALUES
('nojus2041@', '$2a$11$IW6OJy6e2bvR1', 'nojus2041@gmail.com', 'Nojus', '', 'E1089', '+37060000003', 0, 1, 1, 0),
('itsurabus2', '$2a$11$D0iajydiFT4/9', 'itsurabus2@gmail.com', 'dss', 'sddd', 'G8144', '+37062131321', 0, 1, 1, 0),
('daivastulp', '$2a$11$dtB7rKDC90XJ6', 'daivastulpiniene@gmail.com', 'Daiva', 'Stulpiniene', 'L7127', '+37063205454', 0, 1, 1, 0),
('guodastulp', '$2a$11$0kxF/EJqijWZt', 'guodastulpinaite@gmail.com', 'Guoda', 'Stulpinaitė', 'R6375', '+37060000123', 1, 1, 1, 0),
('gucears@gm', '$2a$11$3eBbivUTKYTA5', 'gucears@gmail.com', 'Gucear', 'S', 'Z5506', '+37060000003', 0, 1, 1, 1);

-- --------------------------------------------------------

--
-- Table structure for table `userclass`
--

CREATE TABLE `userclass` (
  `id_UserClass` int(11) NOT NULL,
  `fk_Classcode` varchar(10) NOT NULL,
  `fk_Usercode` varchar(10) NOT NULL,
  `department` int(11) DEFAULT NULL,
  `auditorium` varchar(50) DEFAULT NULL,
  `startTime` datetime DEFAULT NULL,
  `endTime` datetime DEFAULT NULL,
  `teacher` varchar(50) DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  `type` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Dumping data for table `userclass`
--

INSERT INTO `userclass` (`id_UserClass`, `fk_Classcode`, `fk_Usercode`, `department`, `auditorium`, `startTime`, `endTime`, `teacher`, `duration`, `type`) VALUES
(18, 'CLS1740424', 'R6375', 11, '103', '2025-02-25 22:45:00', '2025-02-25 23:45:00', 'Skriaudikas', 60, 2),
(46, 'CLS1740950', 'R6375', 1, '103', '2025-03-03 09:00:00', '2025-03-03 10:00:00', 'NeGiedrius NeGiedraitis', 60, 3),
(47, 'P000B112', 'R6375', 0, '', '2025-02-19 09:00:00', '2025-02-19 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(48, 'P000B112', 'R6375', 0, '', '2025-02-26 09:00:00', '2025-02-26 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(49, 'P000B112', 'R6375', 0, '', '2025-03-05 09:00:00', '2025-03-05 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(54, 'P000B112', 'R6375', 0, '', '2025-04-09 09:00:00', '2025-04-09 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(55, 'P000B112', 'R6375', 0, '', '2025-04-16 09:00:00', '2025-04-16 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(56, 'P000B112', 'R6375', 0, '', '2025-04-30 09:00:00', '2025-04-30 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(57, 'P000B112', 'R6375', 0, '', '2025-05-07 09:00:00', '2025-05-07 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(58, 'P000B112', 'R6375', 0, '', '2025-05-14 09:00:00', '2025-05-14 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(59, 'P000B112', 'R6375', 0, '', '2025-05-21 09:00:00', '2025-05-21 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(60, 'P000B112', 'R6375', 0, '', '2025-05-28 09:00:00', '2025-05-28 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(61, 'CLS1741372', 'R6375', 4, '100', '2025-03-08 09:00:00', '2025-03-08 10:00:00', 'Petras Petraitis', 60, 1),
(62, 'P000B112', 'R6375', 0, '', '2025-03-12 09:00:00', '2025-03-12 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(63, 'P000B112', 'R6375', 1, 'Nuotoliniu būdu', '2025-03-19 09:00:00', '2025-03-19 10:30:00', 'Tomas Skersys', 90, 4),
(65, 'P000B112', 'Z5506', 0, '', '2025-02-19 09:00:00', '2025-02-19 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(66, 'P000B112', 'Z5506', 0, '', '2025-02-26 09:00:00', '2025-02-26 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(67, 'P000B112', 'Z5506', 0, '', '2025-03-05 09:00:00', '2025-03-05 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(68, 'P000B112', 'Z5506', 0, '', '2025-03-12 09:00:00', '2025-03-12 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(69, 'P000B112', 'Z5506', 0, '', '2025-03-19 09:00:00', '2025-03-19 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(70, 'P000B112', 'Z5506', 0, '', '2025-03-26 09:00:00', '2025-03-26 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(72, 'P000B112', 'Z5506', 0, '', '2025-04-09 09:00:00', '2025-04-09 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(73, 'P000B112', 'Z5506', 0, '', '2025-04-16 09:00:00', '2025-04-16 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(74, 'P000B112', 'Z5506', 0, '', '2025-04-30 09:00:00', '2025-04-30 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(75, 'P000B112', 'Z5506', 0, '', '2025-05-07 09:00:00', '2025-05-07 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(76, 'P000B112', 'Z5506', 0, '', '2025-05-14 09:00:00', '2025-05-14 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(77, 'P000B112', 'Z5506', 0, '', '2025-05-21 09:00:00', '2025-05-21 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(78, 'P000B112', 'Z5506', 0, '', '2025-05-28 09:00:00', '2025-05-28 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(79, 'P000B112', 'E1089', 0, '', '2025-02-19 09:00:00', '2025-02-19 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(80, 'P000B112', 'E1089', 0, '', '2025-02-26 09:00:00', '2025-02-26 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(81, 'P000B112', 'E1089', 0, '', '2025-03-05 09:00:00', '2025-03-05 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(82, 'P000B112', 'E1089', 0, '', '2025-03-12 09:00:00', '2025-03-12 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(83, 'P000B112', 'E1089', 0, '', '2025-03-19 09:00:00', '2025-03-19 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(84, 'P000B112', 'E1089', 0, '', '2025-03-26 09:00:00', '2025-03-26 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(85, 'P000B112', 'E1089', 0, '', '2025-04-02 09:00:00', '2025-04-02 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(86, 'P000B112', 'E1089', 0, '', '2025-04-09 09:00:00', '2025-04-09 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(87, 'P000B112', 'E1089', 0, '', '2025-04-16 09:00:00', '2025-04-16 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(88, 'P000B112', 'E1089', 0, '', '2025-04-30 09:00:00', '2025-04-30 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(89, 'P000B112', 'E1089', 0, '', '2025-05-07 09:00:00', '2025-05-07 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(90, 'P000B112', 'E1089', 0, '', '2025-05-14 09:00:00', '2025-05-14 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(91, 'P000B112', 'E1089', 0, '', '2025-05-21 09:00:00', '2025-05-21 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(92, 'P000B112', 'E1089', 0, '', '2025-05-28 09:00:00', '2025-05-28 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(93, 'CLS1742330', 'G8144', 2, '103', '2025-03-19 09:00:00', '2025-03-19 10:00:00', 'Petras Petraitis', 60, 3),
(94, 'T001B000', 'G8144', 2, '100', '2025-03-19 11:00:00', '2025-03-19 12:00:00', 'Jonas Jonaitis', 60, 1),
(95, 'P000B112', 'G8144', 0, '', '2025-02-19 09:00:00', '2025-02-19 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(96, 'P000B112', 'G8144', 0, '', '2025-02-26 09:00:00', '2025-02-26 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(97, 'P000B112', 'G8144', 0, '', '2025-03-05 09:00:00', '2025-03-05 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(98, 'P000B112', 'G8144', 0, '', '2025-03-12 09:00:00', '2025-03-12 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(99, 'P000B112', 'G8144', 0, '', '2025-03-26 09:00:00', '2025-03-26 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(100, 'P000B112', 'G8144', 0, '', '2025-04-02 09:00:00', '2025-04-02 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(101, 'P000B112', 'G8144', 0, '', '2025-04-09 09:00:00', '2025-04-09 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(102, 'P000B112', 'G8144', 0, '', '2025-04-16 09:00:00', '2025-04-16 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(104, 'P000B112', 'G8144', 0, '', '2025-05-07 09:00:00', '2025-05-07 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(105, 'P000B112', 'G8144', 0, '', '2025-05-14 09:00:00', '2025-05-14 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(106, 'P000B112', 'G8144', 0, '', '2025-05-21 09:00:00', '2025-05-21 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(107, 'P000B112', 'G8144', 0, '', '2025-05-28 09:00:00', '2025-05-28 10:30:00', 'Nuotol.būd-; , , Nuotoliniu būdu', 90, 4),
(108, 'CLS1742585', 'R6375', 3, '100', '2025-03-22 10:00:00', '2025-03-22 11:00:00', 'Jonas Jonaitėnas', 60, 3),
(109, 'CLS1743350', 'Z5506', 2, '110', '2025-03-31 10:00:00', '2025-03-31 11:00:00', 'dsfdsf', 60, 1),
(110, 'CLS1744387', 'R6375', 1, '111', '2025-04-12 11:00:00', '2025-04-12 12:00:00', 'Miau miau', 60, 1),
(111, 'CLS1744387', 'R6375', 1, '111', '2025-04-19 11:00:00', '2025-04-19 12:00:00', 'Miau miau', 60, 1),
(112, 'CLS1744387', 'R6375', 1, '111', '2025-04-26 11:00:00', '2025-04-26 12:00:00', 'Miau miau', 60, 1),
(113, 'CLS1744387', 'R6375', 1, '111', '2025-05-03 11:00:00', '2025-05-03 12:00:00', 'Miau miau', 60, 1),
(114, 'CLS1744387', 'R6375', 1, '111', '2025-05-10 11:00:00', '2025-05-10 12:00:00', 'Miau miau', 60, 1),
(115, 'CLS1744387', 'R6375', 1, '111', '2025-05-17 11:00:00', '2025-05-17 12:00:00', 'Miau miau', 60, 1),
(122, 'CLS1744387', 'R6375', 1, 'fsd', '2025-04-12 12:00:00', '2025-04-12 13:00:00', 'dfs', 60, 3),
(123, 'CLS1744387', 'R6375', 1, 'fsd', '2025-04-14 12:00:00', '2025-04-14 13:00:00', 'dfs', 60, 3),
(124, 'CLS1744387', 'R6375', 2, 'sdfsd', '2025-04-12 13:00:00', '2025-04-12 14:00:00', 'fdsfdsff', 60, 2),
(125, 'CLS1744387', 'R6375', 2, 'sdfsd', '2025-04-15 13:00:00', '2025-04-15 14:00:00', 'fdsfdsff', 60, 2),
(126, 'CLS1744387', 'R6375', 2, 'sdfsd', '2025-04-18 13:00:00', '2025-04-18 14:00:00', 'fdsfdsff', 60, 2),
(130, 'CLS1744387', 'R6375', 3, 'A', '2025-05-06 19:00:00', '2025-05-06 20:30:00', 'a', 90, 4),
(131, 'CLS1744387', 'R6375', 3, 'A', '2025-05-12 19:00:00', '2025-05-12 20:30:00', 'a', 90, 4),
(132, 'CLS1744388', 'R6375', 2, '123d', '2025-04-12 14:00:00', '2025-04-12 15:00:00', 'asdads', 60, 1),
(133, 'CLS1744388', 'R6375', 2, '123d', '2025-04-15 14:00:00', '2025-04-15 15:00:00', 'asdads', 60, 1),
(134, 'CLS1744388', 'R6375', 2, '123d', '2025-04-18 14:00:00', '2025-04-18 15:00:00', 'asdads', 60, 1),
(135, 'CLS1744388', 'R6375', 2, '123d', '2025-04-21 14:00:00', '2025-04-21 15:00:00', 'asdads', 60, 1),
(136, 'CLS8AKG364', 'Z5506', 4, '12j', '2025-04-12 11:00:00', '2025-04-12 12:00:00', 'AAAAA AAAAA', 60, 1),
(137, 'CLS8AKG364', 'Z5506', 4, '12j', '2025-04-19 11:00:00', '2025-04-19 12:00:00', 'AAAAA AAAAA', 60, 1),
(138, 'CLS8AKG364', 'Z5506', 4, '12j', '2025-04-26 11:00:00', '2025-04-26 12:00:00', 'AAAAA AAAAA', 60, 1),
(139, 'CLS8AKG364', 'Z5506', 4, '12j', '2025-05-03 11:00:00', '2025-05-03 12:00:00', 'AAAAA AAAAA', 60, 1),
(140, 'CLS8AKG364', 'Z5506', 4, '12j', '2025-05-10 11:00:00', '2025-05-10 12:00:00', 'AAAAA AAAAA', 60, 1),
(141, 'CLS8AKG364', 'Z5506', 4, '12j', '2025-05-17 11:00:00', '2025-05-17 12:00:00', 'AAAAA AAAAA', 60, 1),
(142, 'CLSYD3J07K', 'Z5506', 5, 'A', '2025-04-18 12:30:00', '2025-04-18 13:30:00', 'AAAAAAAAAAAA', 60, 2),
(143, 'CLSYD3J07K', 'Z5506', 5, 'A', '2025-04-25 12:30:00', '2025-04-25 13:30:00', 'AAAAAAAAAAAA', 60, 2),
(144, 'CLSYD3J07K', 'Z5506', 5, 'A', '2025-05-02 12:30:00', '2025-05-02 13:30:00', 'AAAAAAAAAAAA', 60, 2),
(145, 'CLSYD3J07K', 'Z5506', 5, 'A', '2025-05-09 12:30:00', '2025-05-09 13:30:00', 'AAAAAAAAAAAA', 60, 2),
(146, 'CLSYD3J07K', 'Z5506', 5, 'A', '2025-05-16 12:30:00', '2025-05-16 13:30:00', 'AAAAAAAAAAAA', 60, 2),
(147, 'CLSG27SDFC', 'E1089', 1, 'A', '2025-04-12 11:00:00', '2025-04-12 12:00:00', 'A', 60, 2),
(148, 'CLSG27SDFC', 'E1089', 1, 'A', '2025-04-13 11:00:00', '2025-04-13 12:00:00', 'A', 60, 2),
(152, 'CLSYD3J07K', 'E1089', 2, 'A', '2025-04-18 12:30:00', '2025-04-18 13:30:00', 'AAAAAAAAAAAA', 60, 5),
(153, 'CLSYD3J07K', 'E1089', 2, 'A', '2025-04-25 12:30:00', '2025-04-25 13:30:00', 'AAAAAAAAAAAA', 60, 5),
(154, 'CLSYD3J07K', 'E1089', 2, 'A', '2025-05-02 12:30:00', '2025-05-02 13:30:00', 'AAAAAAAAAAAA', 60, 5),
(157, 'CLS8AKG364', 'E1089', 1, '12j', '2025-04-26 11:00:00', '2025-04-26 12:00:00', 'AAAAA AAAAA', 60, 5),
(158, 'CLS8AKG364', 'E1089', 1, '12j', '2025-04-28 11:00:00', '2025-04-28 12:00:00', 'AAAAA AAAAA', 60, 5),
(159, 'CLSOIB4VZ3', 'E1089', 1, 'A', '2025-04-15 10:00:00', '2025-04-15 11:00:00', 'A', 60, 2),
(160, 'CLSVYL3BY3', 'E1089', 2, 'BB', '2025-04-15 11:00:00', '2025-04-15 12:00:00', 'B', 60, 2),
(161, 'CLSAUIEU42', 'E1089', 2, 'dfggf', '2025-04-15 12:00:00', '2025-04-15 13:00:00', 'sdasd', 60, 1),
(162, 'CLS8AKG364', 'E1089', 1, '12j', '2025-05-03 11:00:00', '2025-05-03 12:00:00', 'AAAAA AAAAA', 60, 5),
(163, 'CLS8AKG364', 'E1089', 1, '12j', '2025-05-10 11:00:00', '2025-05-10 12:00:00', 'AAAAA AAAAA', 60, 5),
(164, 'CLS8AKG364', 'E1089', 1, '12j', '2025-05-17 11:00:00', '2025-05-17 12:00:00', 'AAAAA AAAAA', 60, 5),
(165, 'CLS8AKG364', 'E1089', 1, '12j', '2025-05-24 11:00:00', '2025-05-24 12:00:00', 'AAAAA AAAAA', 60, 5),
(166, 'CLS8AKG364', 'E1089', 1, '12j', '2025-05-31 11:00:00', '2025-05-31 12:00:00', 'AAAAA AAAAA', 60, 5),
(167, 'CLS8AKG364', 'E1089', 1, '12j', '2025-06-07 11:00:00', '2025-06-07 12:00:00', 'AAAAA AAAAA', 60, 5),
(168, 'CLS8AKG364', 'E1089', 1, '12j', '2025-06-14 11:00:00', '2025-06-14 12:00:00', 'AAAAA AAAAA', 60, 5);

-- --------------------------------------------------------

--
-- Table structure for table `vicedeadforstudies`
--

CREATE TABLE `vicedeadforstudies` (
  `code` varchar(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_lithuanian_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `affair`
--
ALTER TABLE `affair`
  ADD PRIMARY KEY (`id_Affair`),
  ADD KEY `event_id` (`event_id`),
  ADD KEY `event_id_2` (`event_id`);

--
-- Indexes for table `class`
--
ALTER TABLE `class`
  ADD PRIMARY KEY (`code`);

--
-- Indexes for table `classtypes`
--
ALTER TABLE `classtypes`
  ADD PRIMARY KEY (`id_ClassTypes`);

--
-- Indexes for table `departments`
--
ALTER TABLE `departments`
  ADD PRIMARY KEY (`id_Departments`);

--
-- Indexes for table `event`
--
ALTER TABLE `event`
  ADD PRIMARY KEY (`id_Event`);

--
-- Indexes for table `feedback`
--
ALTER TABLE `feedback`
  ADD PRIMARY KEY (`id_Feedback`),
  ADD KEY `acquires` (`fk_Affairid_Affair`),
  ADD KEY `gets` (`fk_Trainingid_Training`),
  ADD KEY `fk_feedback_semesterplantask` (`fk_SemesterPlanTask`);

--
-- Indexes for table `mentee`
--
ALTER TABLE `mentee`
  ADD PRIMARY KEY (`code`),
  ADD KEY `studyProgram` (`studyProgram`),
  ADD KEY `fk_mentee_specialization` (`specialization`),
  ADD KEY `FK_Mentee_Mentor` (`MentorCode`);

--
-- Indexes for table `mentor`
--
ALTER TABLE `mentor`
  ADD PRIMARY KEY (`code`),
  ADD KEY `department` (`department`);

--
-- Indexes for table `mentorexpertise`
--
ALTER TABLE `mentorexpertise`
  ADD PRIMARY KEY (`id`),
  ADD KEY `IX_MentorExpertise_MentorCode` (`mentorCode`),
  ADD KEY `FK_MentorExpertise_StudyProgram` (`studyProgram`),
  ADD KEY `FK_MentorExpertise_Specialization` (`specialization`);

--
-- Indexes for table `mentorrequest`
--
ALTER TABLE `mentorrequest`
  ADD PRIMARY KEY (`id`),
  ADD KEY `mentorCode` (`mentorCode`),
  ADD KEY `menteeCode` (`menteeCode`),
  ADD KEY `FK_MentorRequest_RequestStatus` (`RequestStatusId`);

--
-- Indexes for table `mentorsevent`
--
ALTER TABLE `mentorsevent`
  ADD PRIMARY KEY (`id_MentorEvent`),
  ADD KEY `isArranged` (`fk_Eventid_Event`),
  ADD KEY `arranges` (`fk_Mentorcode`);

--
-- Indexes for table `rejectionhistory`
--
ALTER TABLE `rejectionhistory`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_user` (`userCode`);

--
-- Indexes for table `requeststatuses`
--
ALTER TABLE `requeststatuses`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `semesterplan`
--
ALTER TABLE `semesterplan`
  ADD PRIMARY KEY (`id_SemesterPlan`),
  ADD UNIQUE KEY `fk_Menteecode` (`fk_Menteecode`),
  ADD KEY `concludes` (`fk_Mentorcode`);

--
-- Indexes for table `semesterplanclass`
--
ALTER TABLE `semesterplanclass`
  ADD PRIMARY KEY (`id_SemesterPlanClass`),
  ADD KEY `includes` (`fk_SemesterPlanid_SemesterPlan`),
  ADD KEY `isIncluded` (`fk_Classcode`);

--
-- Indexes for table `semesterplanevent`
--
ALTER TABLE `semesterplanevent`
  ADD PRIMARY KEY (`id_SemesterPlanEvent`),
  ADD KEY `incorporates` (`fk_SemesterPlanid_SemesterPlan`),
  ADD KEY `isIncorporated` (`fk_Eventid_Event`);

--
-- Indexes for table `semesterplantask`
--
ALTER TABLE `semesterplantask`
  ADD PRIMARY KEY (`id_SemesterPlanTask`),
  ADD KEY `adds` (`fk_SemesterPlanid_SemesterPlan`),
  ADD KEY `isAdded` (`fk_Taskid_Task`);

--
-- Indexes for table `specializations`
--
ALTER TABLE `specializations`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_specializations_studyprogram` (`fk_id_StudyPrograms`);

--
-- Indexes for table `studyprograms`
--
ALTER TABLE `studyprograms`
  ADD PRIMARY KEY (`id_StudyPrograms`);

--
-- Indexes for table `task`
--
ALTER TABLE `task`
  ADD PRIMARY KEY (`id_Task`),
  ADD KEY `fk_task_createdBy` (`createdBy`),
  ADD KEY `fk_task_tasktypes` (`taskTypeId`),
  ADD KEY `fk_task_event` (`fk_Eventid_Event`);

--
-- Indexes for table `tasktypes`
--
ALTER TABLE `tasktypes`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `training`
--
ALTER TABLE `training`
  ADD PRIMARY KEY (`id_Training`),
  ADD KEY `organizes` (`fk_ViceDeadForStudiescode`),
  ADD KEY `fk_training_event` (`event_id`);

--
-- Indexes for table `user`
--
ALTER TABLE `user`
  ADD PRIMARY KEY (`code`);

--
-- Indexes for table `userclass`
--
ALTER TABLE `userclass`
  ADD PRIMARY KEY (`id_UserClass`),
  ADD KEY `assigned` (`fk_Classcode`),
  ADD KEY `attends` (`fk_Usercode`),
  ADD KEY `type` (`type`);

--
-- Indexes for table `vicedeadforstudies`
--
ALTER TABLE `vicedeadforstudies`
  ADD PRIMARY KEY (`code`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `affair`
--
ALTER TABLE `affair`
  MODIFY `id_Affair` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT for table `event`
--
ALTER TABLE `event`
  MODIFY `id_Event` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=29;

--
-- AUTO_INCREMENT for table `feedback`
--
ALTER TABLE `feedback`
  MODIFY `id_Feedback` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT for table `mentorexpertise`
--
ALTER TABLE `mentorexpertise`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=51;

--
-- AUTO_INCREMENT for table `mentorrequest`
--
ALTER TABLE `mentorrequest`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=31;

--
-- AUTO_INCREMENT for table `mentorsevent`
--
ALTER TABLE `mentorsevent`
  MODIFY `id_MentorEvent` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `rejectionhistory`
--
ALTER TABLE `rejectionhistory`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT for table `semesterplan`
--
ALTER TABLE `semesterplan`
  MODIFY `id_SemesterPlan` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `semesterplanclass`
--
ALTER TABLE `semesterplanclass`
  MODIFY `id_SemesterPlanClass` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `semesterplanevent`
--
ALTER TABLE `semesterplanevent`
  MODIFY `id_SemesterPlanEvent` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- AUTO_INCREMENT for table `semesterplantask`
--
ALTER TABLE `semesterplantask`
  MODIFY `id_SemesterPlanTask` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=32;

--
-- AUTO_INCREMENT for table `specializations`
--
ALTER TABLE `specializations`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT for table `task`
--
ALTER TABLE `task`
  MODIFY `id_Task` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=27;

--
-- AUTO_INCREMENT for table `tasktypes`
--
ALTER TABLE `tasktypes`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `training`
--
ALTER TABLE `training`
  MODIFY `id_Training` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT for table `userclass`
--
ALTER TABLE `userclass`
  MODIFY `id_UserClass` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=169;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `affair`
--
ALTER TABLE `affair`
  ADD CONSTRAINT `fk_affair_event` FOREIGN KEY (`event_id`) REFERENCES `event` (`id_Event`);

--
-- Constraints for table `feedback`
--
ALTER TABLE `feedback`
  ADD CONSTRAINT `acquires` FOREIGN KEY (`fk_Affairid_Affair`) REFERENCES `event` (`id_Event`),
  ADD CONSTRAINT `fk_feedback_semesterplantask` FOREIGN KEY (`fk_SemesterPlanTask`) REFERENCES `semesterplantask` (`id_SemesterPlanTask`),
  ADD CONSTRAINT `gets` FOREIGN KEY (`fk_Trainingid_Training`) REFERENCES `training` (`id_Training`);

--
-- Constraints for table `mentee`
--
ALTER TABLE `mentee`
  ADD CONSTRAINT `FK_Mentee_Mentor` FOREIGN KEY (`MentorCode`) REFERENCES `mentor` (`code`) ON DELETE SET NULL,
  ADD CONSTRAINT `fk_mentee_specialization` FOREIGN KEY (`specialization`) REFERENCES `specializations` (`id`) ON DELETE SET NULL,
  ADD CONSTRAINT `mentee_ibfk_1` FOREIGN KEY (`studyProgram`) REFERENCES `studyprograms` (`id_StudyPrograms`),
  ADD CONSTRAINT `mentee_ibfk_2` FOREIGN KEY (`code`) REFERENCES `user` (`code`) ON DELETE CASCADE;

--
-- Constraints for table `mentor`
--
ALTER TABLE `mentor`
  ADD CONSTRAINT `mentor_ibfk_1` FOREIGN KEY (`department`) REFERENCES `departments` (`id_Departments`),
  ADD CONSTRAINT `mentor_ibfk_2` FOREIGN KEY (`code`) REFERENCES `user` (`code`) ON DELETE CASCADE;

--
-- Constraints for table `mentorexpertise`
--
ALTER TABLE `mentorexpertise`
  ADD CONSTRAINT `FK_MentorExpertise_Mentor` FOREIGN KEY (`mentorCode`) REFERENCES `mentor` (`code`),
  ADD CONSTRAINT `FK_MentorExpertise_Specialization` FOREIGN KEY (`specialization`) REFERENCES `specializations` (`id`) ON DELETE SET NULL,
  ADD CONSTRAINT `FK_MentorExpertise_StudyProgram` FOREIGN KEY (`studyProgram`) REFERENCES `studyprograms` (`id_StudyPrograms`);

--
-- Constraints for table `mentorrequest`
--
ALTER TABLE `mentorrequest`
  ADD CONSTRAINT `FK_MentorRequest_Mentee` FOREIGN KEY (`menteeCode`) REFERENCES `mentee` (`code`),
  ADD CONSTRAINT `FK_MentorRequest_Mentor` FOREIGN KEY (`mentorCode`) REFERENCES `mentor` (`code`),
  ADD CONSTRAINT `FK_MentorRequest_RequestStatus` FOREIGN KEY (`RequestStatusId`) REFERENCES `requeststatuses` (`Id`);

--
-- Constraints for table `mentorsevent`
--
ALTER TABLE `mentorsevent`
  ADD CONSTRAINT `fk_mentorsEvent_event` FOREIGN KEY (`fk_Eventid_Event`) REFERENCES `event` (`id_Event`),
  ADD CONSTRAINT `fk_mentorsEvent_mentor` FOREIGN KEY (`fk_Mentorcode`) REFERENCES `mentor` (`code`);

--
-- Constraints for table `rejectionhistory`
--
ALTER TABLE `rejectionhistory`
  ADD CONSTRAINT `fk_user` FOREIGN KEY (`userCode`) REFERENCES `user` (`code`);

--
-- Constraints for table `semesterplan`
--
ALTER TABLE `semesterplan`
  ADD CONSTRAINT `concludes` FOREIGN KEY (`fk_Mentorcode`) REFERENCES `mentor` (`code`),
  ADD CONSTRAINT `has` FOREIGN KEY (`fk_Menteecode`) REFERENCES `mentee` (`code`);

--
-- Constraints for table `semesterplanclass`
--
ALTER TABLE `semesterplanclass`
  ADD CONSTRAINT `includes` FOREIGN KEY (`fk_SemesterPlanid_SemesterPlan`) REFERENCES `semesterplan` (`id_SemesterPlan`),
  ADD CONSTRAINT `isIncluded` FOREIGN KEY (`fk_Classcode`) REFERENCES `class` (`code`);

--
-- Constraints for table `semesterplanevent`
--
ALTER TABLE `semesterplanevent`
  ADD CONSTRAINT `incorporates` FOREIGN KEY (`fk_SemesterPlanid_SemesterPlan`) REFERENCES `semesterplan` (`id_SemesterPlan`),
  ADD CONSTRAINT `isIncorporated` FOREIGN KEY (`fk_Eventid_Event`) REFERENCES `event` (`id_Event`);

--
-- Constraints for table `semesterplantask`
--
ALTER TABLE `semesterplantask`
  ADD CONSTRAINT `adds` FOREIGN KEY (`fk_SemesterPlanid_SemesterPlan`) REFERENCES `semesterplan` (`id_SemesterPlan`),
  ADD CONSTRAINT `isAdded` FOREIGN KEY (`fk_Taskid_Task`) REFERENCES `task` (`id_Task`);

--
-- Constraints for table `specializations`
--
ALTER TABLE `specializations`
  ADD CONSTRAINT `fk_specializations_studyprogram` FOREIGN KEY (`fk_id_StudyPrograms`) REFERENCES `studyprograms` (`id_StudyPrograms`) ON DELETE CASCADE;

--
-- Constraints for table `task`
--
ALTER TABLE `task`
  ADD CONSTRAINT `fk_task_createdBy` FOREIGN KEY (`createdBy`) REFERENCES `user` (`code`),
  ADD CONSTRAINT `fk_task_event` FOREIGN KEY (`fk_Eventid_Event`) REFERENCES `event` (`id_Event`),
  ADD CONSTRAINT `fk_task_tasktypes` FOREIGN KEY (`taskTypeId`) REFERENCES `tasktypes` (`id`) ON DELETE SET NULL;

--
-- Constraints for table `training`
--
ALTER TABLE `training`
  ADD CONSTRAINT `FK_Training_User` FOREIGN KEY (`fk_ViceDeadForStudiescode`) REFERENCES `user` (`code`),
  ADD CONSTRAINT `fk_training_event` FOREIGN KEY (`event_id`) REFERENCES `event` (`id_Event`);

--
-- Constraints for table `userclass`
--
ALTER TABLE `userclass`
  ADD CONSTRAINT `assigned` FOREIGN KEY (`fk_Classcode`) REFERENCES `class` (`code`),
  ADD CONSTRAINT `attends` FOREIGN KEY (`fk_Usercode`) REFERENCES `user` (`code`),
  ADD CONSTRAINT `class_ibfk_1` FOREIGN KEY (`type`) REFERENCES `classtypes` (`id_ClassTypes`);

--
-- Constraints for table `vicedeadforstudies`
--
ALTER TABLE `vicedeadforstudies`
  ADD CONSTRAINT `vicedeadforstudies_ibfk_1` FOREIGN KEY (`code`) REFERENCES `user` (`code`),
  ADD CONSTRAINT `vicedeadforstudies_ibfk_2` FOREIGN KEY (`code`) REFERENCES `mentor` (`code`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
