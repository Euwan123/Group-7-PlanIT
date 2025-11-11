-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Nov 11, 2025 at 01:31 AM
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
-- Database: `planit`
--

-- --------------------------------------------------------

--
-- Table structure for table `checklists`
--

CREATE TABLE `checklists` (
  `id` int(11) NOT NULL,
  `status` tinyint(1) DEFAULT 0,
  `text` varchar(500) DEFAULT NULL,
  `priority` varchar(20) DEFAULT 'Medium',
  `category` varchar(100) DEFAULT NULL,
  `due_date` date DEFAULT NULL,
  `notes` text DEFAULT NULL,
  `item_order` int(11) DEFAULT 0,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `checklists`
--

INSERT INTO `checklists` (`id`, `status`, `text`, `priority`, `category`, `due_date`, `notes`, `item_order`, `created_at`) VALUES
(1, 0, 'Run', 'Medium', 'Work', '2025-11-12', 'I should run 20 miles', 0, '2025-11-07 08:40:28');

-- --------------------------------------------------------

--
-- Table structure for table `dictionary`
--

CREATE TABLE `dictionary` (
  `id` int(11) NOT NULL,
  `word` varchar(255) NOT NULL,
  `definition` text NOT NULL,
  `category` varchar(100) DEFAULT 'General',
  `created_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Dumping data for table `dictionary`
--

INSERT INTO `dictionary` (`id`, `word`, `definition`, `category`, `created_at`) VALUES
(1, 'Plan', 'A detailed proposal for doing or achieving something; a scheme or method of acting.', 'Productivity', '2025-11-02 09:56:57'),
(2, 'Organize', 'To arrange systematically; to order and structure elements efficiently.', 'Productivity', '2025-11-02 09:56:57'),
(3, 'Schedule', 'A plan for carrying out a process or procedure, giving lists of intended events and times.', 'Productivity', '2025-11-02 09:56:57'),
(4, 'Task', 'A piece of work to be done or undertaken; an assignment or duty.', 'Productivity', '2025-11-02 09:56:57'),
(5, 'Goal', 'The object of a person\'s ambition or effort; an aim or desired result.', 'Productivity', '2025-11-02 09:56:57'),
(6, 'Priority', 'A thing that is regarded as more important than others; precedence in order or importance.', 'Productivity', '2025-11-02 09:56:57'),
(7, 'Deadline', 'The latest time or date by which something should be completed.', 'Productivity', '2025-11-02 09:56:57'),
(8, 'Productivity', 'The effectiveness of productive effort, especially measured in terms of output per unit.', 'Productivity', '2025-11-02 09:56:57'),
(9, 'Efficiency', 'The state or quality of being efficient; achieving maximum output with minimum wasted effort.', 'Productivity', '2025-11-02 09:56:57'),
(10, 'Focus', 'The center of interest or activity; to pay particular attention to something.', 'Productivity', '2025-11-02 09:56:57'),
(11, 'Strategy', 'A plan of action designed to achieve a long-term or overall aim.', 'Productivity', '2025-11-02 09:56:57'),
(12, 'Objective', 'A goal or target to be achieved; something aimed at or sought after.', 'Productivity', '2025-11-02 09:56:57'),
(13, 'Milestone', 'A significant stage or event in the development of something.', 'Productivity', '2025-11-02 09:56:57'),
(14, 'Progress', 'Forward or onward movement toward a destination or goal.', 'Productivity', '2025-11-02 09:56:57'),
(15, 'Achievement', 'A thing done successfully with effort, skill, or courage.', 'Productivity', '2025-11-02 09:56:58'),
(16, 'Commitment', 'The state or quality of being dedicated to a cause or activity.', 'Productivity', '2025-11-02 09:56:58'),
(17, 'Consistency', 'Conformity in the application of something; steadfast adherence to principles.', 'Productivity', '2025-11-02 09:56:58'),
(18, 'Delegation', 'The assignment of responsibility or authority to another person to carry out specific activities.', 'Productivity', '2025-11-02 09:56:58'),
(19, 'Execution', 'The carrying out of a plan, order, or course of action.', 'Productivity', '2025-11-02 09:56:58'),
(20, 'Initiative', 'The ability to assess and initiate things independently; taking action proactively.', 'Productivity', '2025-11-02 09:56:58'),
(21, 'Management', 'The process of dealing with or controlling things or people.', 'Productivity', '2025-11-02 09:56:58'),
(22, 'Optimization', 'The action of making the best or most effective use of a situation or resource.', 'Productivity', '2025-11-02 09:56:58'),
(23, 'Performance', 'The accomplishment of a given task measured against preset standards.', 'Productivity', '2025-11-02 09:56:58'),
(24, 'Planning', 'The process of making plans for something; preparation and organization.', 'Productivity', '2025-11-02 09:56:58'),
(25, 'Quality', 'The standard of something as measured against other things; degree of excellence.', 'Productivity', '2025-11-02 09:56:58'),
(26, 'Resource', 'A stock or supply of materials, staff, or assets that can be drawn upon.', 'Productivity', '2025-11-02 09:56:58'),
(27, 'Success', 'The accomplishment of an aim or purpose; favorable outcome of an undertaking.', 'Productivity', '2025-11-02 09:56:58'),
(28, 'Timeline', 'A graphical representation of a period of time showing events in chronological order.', 'Productivity', '2025-11-02 09:56:58'),
(29, 'Vision', 'The ability to think about or plan the future with imagination and wisdom.', 'Productivity', '2025-11-02 09:56:58'),
(30, 'Workflow', 'The sequence of processes through which a piece of work passes from initiation to completion.', 'Productivity', '2025-11-02 09:56:58'),
(31, 'Motivation', 'The reason or reasons one has for acting or behaving in a particular way.', 'Productivity', '2025-11-02 11:08:16'),
(32, 'Persistence', 'Continuing firmly or obstinately in a course of action despite difficulty.', 'Productivity', '2025-11-02 11:08:16'),
(33, 'Innovation', 'The act of introducing new ideas, methods, or products.', 'Creativity', '2025-11-02 11:08:16'),
(34, 'Discipline', 'The practice of training oneself to follow rules or codes of behavior.', 'Self-Development', '2025-11-02 11:08:16'),
(35, 'Collaboration', 'The action of working with someone to produce or create something.', 'Teamwork', '2025-11-02 11:08:16'),
(472, 'Visionary', 'Having or showing clear ideas about what should happen or be done in the future.', 'Leadership', '2025-11-02 11:16:56'),
(473, 'Momentum', 'The strength or force gained by motion or a series of actions.', 'Motivation', '2025-11-02 11:16:56'),
(474, 'Ambition', 'A strong desire to achieve success or distinction.', 'Motivation', '2025-11-02 11:16:56'),
(475, 'Precision', 'The quality of being exact, accurate, and careful.', 'Work Ethics', '2025-11-02 11:16:56'),
(476, 'Integrity', 'The quality of being honest and having strong moral principles.', 'Values', '2025-11-02 11:16:56'),
(477, 'Confidence', 'Belief in one’s abilities or qualities.', 'Self-Development', '2025-11-02 11:16:56'),
(478, 'Adaptability', 'The ability to adjust to new conditions or environments.', 'Self-Development', '2025-11-02 11:16:56'),
(479, 'Empathy', 'The ability to understand and share another person’s feelings.', 'Character', '2025-11-02 11:16:56'),
(480, 'Accountability', 'Being responsible and answerable for actions.', 'Work Ethics', '2025-11-02 11:16:56'),
(481, 'Reliability', 'The quality of being dependable and consistent.', 'Work Ethics', '2025-11-02 11:16:56'),
(482, 'Resilience', 'The capacity to recover quickly from difficulties and setbacks.', 'Motivation', '2025-11-02 11:20:09'),
(483, 'Perseverance', 'Continued effort despite challenges or delay in success.', 'Work Ethics', '2025-11-02 11:20:09'),
(484, 'Creativity', 'The use of imagination or original ideas to create something.', 'Creativity', '2025-11-02 11:20:09'),
(485, 'Determination', 'Firmness of purpose; resoluteness.', 'Motivation', '2025-11-02 11:20:09'),
(486, 'Honesty', 'The quality of being truthful and sincere.', 'Values', '2025-11-02 11:20:09'),
(487, 'Respect', 'Admiration and regard for the feelings or rights of others.', 'Character', '2025-11-02 11:20:09'),
(488, 'Courage', 'The ability to face fear, pain, or adversity with bravery.', 'Character', '2025-11-02 11:20:09'),
(489, 'Patience', 'The ability to accept or tolerate delay without frustration.', 'Character', '2025-11-02 11:20:09'),
(490, 'Gratitude', 'The quality of being thankful and showing appreciation.', 'Values', '2025-11-02 11:20:09'),
(491, 'Optimism', 'Hopefulness and confidence about the future.', 'Mindset', '2025-11-02 11:20:09'),
(492, 'Leadership', 'The action of guiding or directing a group toward a goal.', 'Leadership', '2025-11-02 11:20:09'),
(493, 'Passion', 'Strong enthusiasm or excitement for something.', 'Motivation', '2025-11-02 11:20:09'),
(494, 'Curiosity', 'A strong desire to learn or know more about something.', 'Creativity', '2025-11-02 11:20:09'),
(495, 'Ingenuity', 'The quality of being clever, original, and inventive.', 'Innovation', '2025-11-02 11:20:09'),
(496, 'Diligence', 'Careful and persistent effort to accomplish something.', 'Work Ethics', '2025-11-02 11:20:09'),
(497, 'Flexibility', 'Willingness to change or compromise when necessary.', 'Self-Development', '2025-11-02 11:20:09'),
(498, 'Tenacity', 'Persistence in maintaining or seeking something valued.', 'Motivation', '2025-11-02 11:20:09'),
(499, 'Humility', 'A modest view of one’s importance.', 'Values', '2025-11-02 11:20:09'),
(500, 'Sincerity', 'The quality of being genuine or free from pretense.', 'Character', '2025-11-02 11:20:09'),
(501, 'Gratification', 'A feeling of pleasure or satisfaction.', 'Mindset', '2025-11-02 11:20:09'),
(502, 'Wisdom', 'The ability to make sound decisions based on knowledge and experience.', 'Values', '2025-11-02 11:20:09'),
(503, 'Kindness', 'The quality of being friendly, generous, and considerate.', 'Character', '2025-11-02 11:20:09'),
(504, 'Compassion', 'Sympathetic concern for the sufferings of others.', 'Character', '2025-11-02 11:20:09'),
(505, 'Trustworthiness', 'The ability to be relied on as honest and truthful.', 'Values', '2025-11-02 11:20:09'),
(506, 'Excellence', 'The quality of being outstanding or extremely good.', 'Success', '2025-11-02 11:20:09'),
(507, 'Learning', 'The process of acquiring knowledge or skills.', 'Self-Development', '2025-11-02 11:20:09'),
(508, 'Discretion', 'The ability to make responsible decisions.', 'Character', '2025-11-02 11:20:09'),
(509, 'Tolerance', 'Willingness to accept others’ opinions or beliefs.', 'Values', '2025-11-02 11:20:09'),
(510, 'Balance', 'A state of equilibrium between different aspects of life.', 'Self-Development', '2025-11-02 11:20:09'),
(511, 'Assertiveness', 'Confidence without aggression in expressing opinions.', 'Leadership', '2025-11-02 11:20:09'),
(512, 'Altruism', 'Selfless concern for the well-being of others.', 'Values', '2025-11-02 11:20:09'),
(513, 'Charisma', 'Personal charm that inspires devotion in others.', 'Leadership', '2025-11-02 11:20:09'),
(514, 'Endurance', 'The power to withstand hardship or stress.', 'Motivation', '2025-11-02 11:20:09'),
(515, 'Positivity', 'The practice of being optimistic and confident.', 'Mindset', '2025-11-02 11:20:09'),
(516, 'Drive', 'Determination to achieve a goal.', 'Motivation', '2025-11-02 11:20:09'),
(517, 'Empowerment', 'Giving others confidence and authority.', 'Leadership', '2025-11-02 11:20:09'),
(518, 'Inspiration', 'The process of being mentally stimulated to do something creative.', 'Creativity', '2025-11-02 11:20:09'),
(519, 'Authenticity', 'Being genuine and true to one’s character.', 'Values', '2025-11-02 11:20:09'),
(520, 'Open-mindedness', 'Willingness to consider new ideas.', 'Self-Development', '2025-11-02 11:20:09'),
(521, 'Rationality', 'Being based on reason or logic.', 'Mindset', '2025-11-02 11:20:09'),
(522, 'Self-awareness', 'Conscious knowledge of one’s character and feelings.', 'Self-Development', '2025-11-02 11:20:09'),
(523, 'Mindfulness', 'Awareness of the present moment.', 'Mindset', '2025-11-02 11:20:09'),
(524, 'Organization', 'Systematic arrangement for efficiency.', 'Work Ethics', '2025-11-02 11:20:09'),
(525, 'Clarity', 'The quality of being coherent and easy to understand.', 'Success', '2025-11-02 11:20:09'),
(526, 'Synergy', 'The combined effect greater than the sum of parts.', 'Leadership', '2025-11-02 11:20:09'),
(527, 'Resourcefulness', 'The ability to find quick and clever ways to overcome difficulties.', 'Innovation', '2025-11-02 11:20:09'),
(528, 'Teamwork', 'Cooperative effort toward a common goal.', 'Work Ethics', '2025-11-02 11:20:09'),
(581, 'Tree', 'A tall plant with a trunk and branches made of wood.', 'Nature', '2025-11-02 11:22:58'),
(582, 'River', 'A large natural stream of water flowing to the sea.', 'Nature', '2025-11-02 11:22:58'),
(583, 'Mountain', 'A large natural elevation of the earth’s surface.', 'Nature', '2025-11-02 11:22:58'),
(584, 'Ocean', 'A vast body of salt water covering most of the earth.', 'Nature', '2025-11-02 11:22:58'),
(585, 'Cloud', 'A visible mass of water vapor in the sky.', 'Nature', '2025-11-02 11:22:58'),
(586, 'Rain', 'Moisture condensed from the atmosphere that falls to earth.', 'Nature', '2025-11-02 11:22:58'),
(587, 'Wind', 'The natural movement of air in the atmosphere.', 'Nature', '2025-11-02 11:22:58'),
(588, 'Stone', 'A hard, solid, nonmetallic mineral matter.', 'Objects', '2025-11-02 11:22:58'),
(589, 'Fire', 'The visible effect of combustion producing heat and light.', 'Nature', '2025-11-02 11:22:58'),
(590, 'Flower', 'The colorful reproductive part of a plant.', 'Nature', '2025-11-02 11:22:58'),
(591, 'Leaf', 'A flat, green part of a plant attached to a stem.', 'Nature', '2025-11-02 11:22:58'),
(592, 'Grass', 'Vegetation consisting of small green plants.', 'Nature', '2025-11-02 11:22:58'),
(593, 'Sun', 'The star at the center of our solar system.', 'Nature', '2025-11-02 11:22:58'),
(594, 'Moon', 'The natural satellite of the earth.', 'Nature', '2025-11-02 11:22:58'),
(595, 'Star', 'A luminous point in the night sky that is a distant sun.', 'Nature', '2025-11-02 11:22:58'),
(596, 'Sky', 'The region of the atmosphere and outer space seen from earth.', 'Nature', '2025-11-02 11:22:58'),
(597, 'Book', 'A set of printed pages bound together.', 'Objects', '2025-11-02 11:22:58'),
(598, 'Pen', 'An instrument used for writing or drawing with ink.', 'Objects', '2025-11-02 11:22:58'),
(599, 'Paper', 'Material used for writing, printing, or drawing.', 'Objects', '2025-11-02 11:22:58'),
(600, 'Chair', 'A piece of furniture for one person to sit on.', 'Objects', '2025-11-02 11:22:58'),
(601, 'Table', 'A piece of furniture with a flat top and one or more legs.', 'Objects', '2025-11-02 11:22:58'),
(602, 'Window', 'An opening in a wall to let in light or air.', 'Objects', '2025-11-02 11:22:58'),
(603, 'Door', 'A hinged or sliding barrier that allows entry or exit.', 'Objects', '2025-11-02 11:22:58'),
(604, 'House', 'A building for human habitation.', 'Objects', '2025-11-02 11:22:58'),
(605, 'School', 'An institution for educating children or students.', 'General', '2025-11-02 11:22:58'),
(606, 'Teacher', 'A person who helps others acquire knowledge.', 'General', '2025-11-02 11:22:58'),
(607, 'Student', 'A person who is learning at an educational institution.', 'General', '2025-11-02 11:22:58'),
(608, 'Computer', 'An electronic device for storing and processing data.', 'Technology', '2025-11-02 11:22:58'),
(609, 'Phone', 'A device used to transmit sound over distances.', 'Technology', '2025-11-02 11:22:58'),
(610, 'Keyboard', 'A panel of keys used for typing on a computer.', 'Technology', '2025-11-02 11:22:58'),
(611, 'Mouse', 'A handheld device used to control a computer cursor.', 'Technology', '2025-11-02 11:22:58'),
(612, 'Internet', 'A global computer network providing information access.', 'Technology', '2025-11-02 11:22:58'),
(613, 'Email', 'Messages sent electronically over a computer network.', 'Technology', '2025-11-02 11:22:58'),
(614, 'Battery', 'A device that stores and provides electrical energy.', 'Technology', '2025-11-02 11:22:58'),
(615, 'Screen', 'A flat surface on which images or data are displayed.', 'Technology', '2025-11-02 11:22:58'),
(616, 'Camera', 'A device used to capture images or videos.', 'Technology', '2025-11-02 11:22:58'),
(617, 'Car', 'A road vehicle powered by an engine.', 'Objects', '2025-11-02 11:22:58'),
(618, 'Bike', 'A two-wheeled vehicle powered by pedals.', 'Objects', '2025-11-02 11:22:58'),
(619, 'Bus', 'A large motor vehicle carrying passengers by road.', 'Objects', '2025-11-02 11:22:58'),
(620, 'Train', 'A series of connected vehicles running on rails.', 'Objects', '2025-11-02 11:22:58'),
(621, 'Airplane', 'A powered flying vehicle with fixed wings.', 'Objects', '2025-11-02 11:22:58'),
(622, 'Road', 'A wide way leading from one place to another.', 'General', '2025-11-02 11:22:58'),
(623, 'City', 'A large and densely populated urban area.', 'General', '2025-11-02 11:22:58'),
(624, 'Country', 'A nation with its own government.', 'General', '2025-11-02 11:22:58'),
(625, 'Village', 'A small community in a rural area.', 'General', '2025-11-02 11:22:58'),
(626, 'Market', 'A place where goods are bought and sold.', 'Daily Life', '2025-11-02 11:22:58'),
(627, 'Shop', 'A place where goods are sold to customers.', 'Daily Life', '2025-11-02 11:22:58'),
(628, 'Money', 'A medium of exchange in the form of coins and notes.', 'Daily Life', '2025-11-02 11:22:58'),
(629, 'Bank', 'A financial institution that manages money and loans.', 'Daily Life', '2025-11-02 11:22:58'),
(630, 'Food', 'Substance consumed to provide nutrition and energy.', 'Daily Life', '2025-11-02 11:22:58'),
(631, 'Water', 'A clear, tasteless liquid essential for life.', 'Nature', '2025-11-02 11:22:58'),
(632, 'Fruit', 'The sweet and fleshy product of a plant containing seeds.', 'Nature', '2025-11-02 11:22:58'),
(633, 'Vegetable', 'A plant or part of a plant used as food.', 'Nature', '2025-11-02 11:22:58'),
(634, 'Bread', 'A food made of flour, water, and yeast baked together.', 'Daily Life', '2025-11-02 11:22:58'),
(635, 'Rice', 'A cereal grain that is a staple food for many cultures.', 'Daily Life', '2025-11-02 11:22:58'),
(636, 'Milk', 'A white liquid produced by mammals as nourishment.', 'Daily Life', '2025-11-02 11:22:58'),
(637, 'Egg', 'An oval object laid by birds, often eaten as food.', 'Daily Life', '2025-11-02 11:22:58'),
(638, 'Fish', 'An aquatic animal with gills and fins.', 'Nature', '2025-11-02 11:22:58'),
(639, 'Dog', 'A domesticated carnivorous mammal often kept as a pet.', 'Animals', '2025-11-02 11:22:58'),
(640, 'Cat', 'A small domesticated carnivorous mammal with soft fur.', 'Animals', '2025-11-02 11:22:58'),
(641, 'Bird', 'A warm-blooded egg-laying animal with feathers and wings.', 'Animals', '2025-11-02 11:22:58'),
(642, 'Horse', 'A large domesticated animal used for riding or work.', 'Animals', '2025-11-02 11:22:58'),
(643, 'Cow', 'A large farm animal raised for milk or meat.', 'Animals', '2025-11-02 11:22:58'),
(644, 'Goat', 'A hardy domesticated animal with horns and a beard.', 'Animals', '2025-11-02 11:22:58'),
(645, 'Sheep', 'A domesticated ruminant animal with a thick woolly coat.', 'Animals', '2025-11-02 11:22:58'),
(646, 'Lion', 'A large wild cat known as the king of the jungle.', 'Animals', '2025-11-02 11:22:58'),
(647, 'Tiger', 'A large wild cat with a striped coat.', 'Animals', '2025-11-02 11:22:58'),
(648, 'Elephant', 'The largest living land animal with a trunk.', 'Animals', '2025-11-02 11:22:58'),
(649, 'Monkey', 'A small to medium-sized primate with a long tail.', 'Animals', '2025-11-02 11:22:58'),
(650, 'Friend', 'A person with whom one has a bond of mutual affection.', 'Emotions', '2025-11-02 11:22:58'),
(651, 'Family', 'A group consisting of parents and children.', 'General', '2025-11-02 11:22:58'),
(652, 'Love', 'An intense feeling of deep affection.', 'Emotions', '2025-11-02 11:22:58'),
(653, 'Happiness', 'The state of being happy.', 'Emotions', '2025-11-02 11:22:58'),
(654, 'Sadness', 'The condition or quality of being unhappy.', 'Emotions', '2025-11-02 11:22:58'),
(655, 'Anger', 'A strong feeling of annoyance or hostility.', 'Emotions', '2025-11-02 11:22:58'),
(656, 'Fear', 'An unpleasant emotion caused by danger or threat.', 'Emotions', '2025-11-02 11:22:58'),
(657, 'Surprise', 'A feeling of mild astonishment or shock.', 'Emotions', '2025-11-02 11:22:58'),
(658, 'Hope', 'A feeling of expectation and desire for something.', 'Emotions', '2025-11-02 11:22:58'),
(659, 'Peace', 'A state of tranquility or freedom from conflict.', 'Values', '2025-11-02 11:22:58'),
(660, 'Dream', 'A series of thoughts or images during sleep or aspiration.', 'Mindset', '2025-11-02 11:22:58'),
(661, 'Sleep', 'A condition of rest for the body and mind.', 'Daily Life', '2025-11-02 11:22:58'),
(662, 'Walk', 'To move at a regular pace by lifting and setting down feet.', 'Actions', '2025-11-02 11:22:58'),
(663, 'Run', 'To move quickly with both feet off the ground at times.', 'Actions', '2025-11-02 11:22:58'),
(664, 'Jump', 'To push oneself off the ground into the air.', 'Actions', '2025-11-02 11:22:58'),
(665, 'Talk', 'To speak in order to exchange ideas.', 'Actions', '2025-11-02 11:22:58'),
(666, 'Listen', 'To give attention to sound or speech.', 'Actions', '2025-11-02 11:22:58'),
(667, 'Read', 'To look at and comprehend written words.', 'Actions', '2025-11-02 11:22:58'),
(668, 'Write', 'To mark letters, words, or symbols on a surface.', 'Actions', '2025-11-02 11:22:58'),
(669, 'Eat', 'To put food into the mouth and chew and swallow it.', 'Actions', '2025-11-02 11:22:58'),
(670, 'Drink', 'To take liquid into the mouth and swallow.', 'Actions', '2025-11-02 11:22:58'),
(671, 'Work', 'To perform a task or job.', 'Daily Life', '2025-11-02 11:22:58'),
(672, 'Play', 'To engage in activity for enjoyment.', 'Daily Life', '2025-11-02 11:22:58'),
(673, 'Rest', 'To relax or cease work for recovery.', 'Daily Life', '2025-11-02 11:22:58'),
(674, 'Smile', 'To form one’s features into a pleased or kind expression.', 'Emotions', '2025-11-02 11:22:58'),
(675, 'Cry', 'To shed tears as an expression of emotion.', 'Emotions', '2025-11-02 11:22:58'),
(676, 'Think', 'To have ideas or opinions in the mind.', 'Mindset', '2025-11-02 11:22:58'),
(677, 'Learn', 'To gain knowledge or skill through study or experience.', 'Self-Development', '2025-11-02 11:22:58'),
(678, 'Logic', 'Reasoning conducted according to strict principles.', 'Philosophy', '2025-11-02 11:24:49'),
(679, 'Justice', 'The quality of being fair and reasonable.', 'Society', '2025-11-02 11:24:49'),
(680, 'Harmony', 'A pleasing arrangement of parts; agreement or peace.', 'Values', '2025-11-02 11:24:49'),
(681, 'Awareness', 'Knowledge or perception of a situation or fact.', 'Mindset', '2025-11-02 11:24:49'),
(682, 'Energy', 'The strength and vitality required for sustained activity.', 'Science', '2025-11-02 11:24:49'),
(683, 'Atom', 'The basic unit of a chemical element.', 'Science', '2025-11-02 11:24:49'),
(684, 'Gravity', 'The natural force that attracts objects toward one another.', 'Science', '2025-11-02 11:24:49'),
(685, 'Orbit', 'The curved path of a celestial object around a star or planet.', 'Astronomy', '2025-11-02 11:24:49'),
(686, 'Galaxy', 'A vast system of stars, gas, and dust held together by gravity.', 'Astronomy', '2025-11-02 11:24:49'),
(687, 'Planet', 'A celestial body orbiting a star.', 'Astronomy', '2025-11-02 11:24:49'),
(688, 'Light', 'Visible energy that makes things able to be seen.', 'Science', '2025-11-02 11:24:49'),
(689, 'Shadow', 'A dark shape formed when light is blocked.', 'Nature', '2025-11-02 11:24:49'),
(690, 'Reflection', 'The return of light or sound waves from a surface.', 'Science', '2025-11-02 11:24:49'),
(691, 'Culture', 'The customs, arts, and social behavior of a society.', 'Society', '2025-11-02 11:24:49'),
(692, 'Tradition', 'A belief or behavior passed down through generations.', 'Society', '2025-11-02 11:24:49'),
(693, 'Language', 'A system of communication using words and symbols.', 'General', '2025-11-02 11:24:49'),
(694, 'History', 'The study of past events.', 'Education', '2025-11-02 11:24:49'),
(695, 'Music', 'The art of arranging sounds in time to produce melody and rhythm.', 'Art', '2025-11-02 11:24:49'),
(696, 'Art', 'The expression of creativity through visual or performing mediums.', 'Art', '2025-11-02 11:24:49'),
(697, 'Poetry', 'Literary work expressing emotion or ideas through rhythm and style.', 'Art', '2025-11-02 11:24:49'),
(698, 'Drama', 'A play for theater, radio, or television.', 'Art', '2025-11-02 11:24:49'),
(699, 'Painting', 'The process or art of using paint to create images.', 'Art', '2025-11-02 11:24:49'),
(700, 'Sculpture', 'The art of creating three-dimensional forms by shaping materials.', 'Art', '2025-11-02 11:24:49'),
(701, 'Comedy', 'Entertainment that provokes laughter.', 'Art', '2025-11-02 11:24:49'),
(702, 'Tragedy', 'A serious drama dealing with sorrowful themes.', 'Art', '2025-11-02 11:24:49'),
(703, 'Imagination', 'The ability to form new ideas not present to the senses.', 'Creativity', '2025-11-02 11:24:49'),
(704, 'Invention', 'The creation of a new device or process.', 'Innovation', '2025-11-02 11:24:49'),
(705, 'Discovery', 'The action of finding something previously unknown.', 'Science', '2025-11-02 11:24:49'),
(706, 'Experiment', 'A scientific procedure to test a hypothesis.', 'Science', '2025-11-02 11:24:49'),
(707, 'Equation', 'A mathematical statement showing equality between expressions.', 'Mathematics', '2025-11-02 11:24:49'),
(708, 'Algorithm', 'A process or set of rules for problem-solving.', 'Technology', '2025-11-02 11:24:49'),
(709, 'Data', 'Facts and statistics collected for reference or analysis.', 'Technology', '2025-11-02 11:24:49'),
(710, 'Network', 'A system of interconnected people or devices.', 'Technology', '2025-11-02 11:24:49'),
(711, 'Signal', 'A transmitted electrical or radio message.', 'Technology', '2025-11-02 11:24:49'),
(712, 'Pixel', 'The smallest unit of a digital image.', 'Technology', '2025-11-02 11:24:49'),
(713, 'Code', 'A system of symbols or rules for communication or programming.', 'Technology', '2025-11-02 11:24:49'),
(714, 'Robot', 'A machine capable of carrying out complex tasks automatically.', 'Technology', '2025-11-02 11:24:49'),
(715, 'Drone', 'A pilotless aircraft controlled remotely or autonomously.', 'Technology', '2025-11-02 11:24:49'),
(716, 'Voltage', 'The electric potential difference between two points.', 'Science', '2025-11-02 11:24:49'),
(717, 'Temperature', 'A measure of the warmth or coldness of an object.', 'Science', '2025-11-02 11:24:49'),
(718, 'Pressure', 'The force exerted on a surface per unit area.', 'Science', '2025-11-02 11:24:49'),
(719, 'Matter', 'Anything that has mass and occupies space.', 'Science', '2025-11-02 11:24:49'),
(720, 'Oxygen', 'A colorless gas essential for life.', 'Science', '2025-11-02 11:24:49'),
(721, 'Carbon', 'A chemical element that forms the basis of organic life.', 'Science', '2025-11-02 11:24:49'),
(722, 'Emotion', 'A natural instinctive state of mind derived from feelings.', 'Emotions', '2025-11-02 11:24:49'),
(723, 'Memory', 'The ability to store and recall information.', 'Mindset', '2025-11-02 11:24:49'),
(724, 'Idea', 'A thought or suggestion as to a possible course of action.', 'Creativity', '2025-11-02 11:24:49'),
(725, 'Talent', 'A natural aptitude or skill.', 'Creativity', '2025-11-02 11:24:49'),
(726, 'Skill', 'The ability to do something well through training or experience.', 'Self-Development', '2025-11-02 11:24:49'),
(727, 'Knowledge', 'Facts, information, and understanding acquired by experience.', 'Education', '2025-11-02 11:24:49'),
(728, 'Freedom', 'The power or right to act, speak, or think as one wants.', 'Values', '2025-11-02 11:24:49'),
(729, 'Power', 'The ability to control or influence others or events.', 'Society', '2025-11-02 11:24:49'),
(730, 'Honor', 'High respect or esteem; adherence to moral principles.', 'Values', '2025-11-02 11:24:49'),
(731, 'Truth', 'The quality of being in accordance with fact or reality.', 'Values', '2025-11-02 11:24:49'),
(732, 'Trust', 'Firm belief in the reliability of someone or something.', 'Values', '2025-11-02 11:24:49'),
(733, 'Charity', 'The voluntary giving of help to those in need.', 'Values', '2025-11-02 11:24:49'),
(734, 'Faith', 'Strong belief in something without proof.', 'Spirituality', '2025-11-02 11:24:49'),
(735, 'Prayer', 'A solemn request or expression of thanks to a deity.', 'Spirituality', '2025-11-02 11:24:49'),
(736, 'Spirit', 'The non-physical part of a person considered immortal.', 'Spirituality', '2025-11-02 11:24:49'),
(737, 'Soul', 'The spiritual or immaterial essence of a human being.', 'Spirituality', '2025-11-02 11:24:49'),
(738, 'Destiny', 'The events that will necessarily happen to a person in the future.', 'Philosophy', '2025-11-02 11:24:49'),
(739, 'Fate', 'The development of events beyond a person’s control.', 'Philosophy', '2025-11-02 11:24:49'),
(740, 'Chance', 'The occurrence of events without apparent cause.', 'Philosophy', '2025-11-02 11:24:49'),
(741, 'Luck', 'Success or failure apparently caused by random chance.', 'Philosophy', '2025-11-02 11:24:49'),
(742, 'Time', 'The indefinite continued progress of existence and events.', 'General', '2025-11-02 11:24:49'),
(743, 'Space', 'The boundless expanse that contains all matter and energy.', 'Science', '2025-11-02 11:24:49'),
(744, 'Universe', 'All existing matter, space, and energy as a whole.', 'Astronomy', '2025-11-02 11:24:49'),
(745, 'Infinity', 'Something without any limit or end.', 'Mathematics', '2025-11-02 11:24:49'),
(746, 'Chaos', 'Complete disorder and confusion.', 'Philosophy', '2025-11-02 11:24:49'),
(747, 'Order', 'The arrangement or disposition of people or things.', 'Philosophy', '2025-11-02 11:24:49'),
(748, 'Structure', 'The arrangement of parts in an object or system.', 'Science', '2025-11-02 11:24:49'),
(749, 'Pattern', 'A repeated decorative or logical design.', 'General', '2025-11-02 11:24:49'),
(750, 'Symbol', 'A thing that represents or stands for something else.', 'General', '2025-11-02 11:24:49'),
(751, 'Concept', 'An abstract idea or general notion.', 'Education', '2025-11-02 11:24:49'),
(752, 'Reality', 'The state of things as they actually exist.', 'Philosophy', '2025-11-02 11:24:49'),
(753, 'Illusion', 'A false idea or belief; something that deceives the senses.', 'Psychology', '2025-11-02 11:24:49'),
(754, 'Habit', 'A regular tendency or practice, especially one hard to give up.', 'Behavior', '2025-11-02 11:24:49'),
(755, 'Decision', 'A conclusion reached after consideration.', 'Behavior', '2025-11-02 11:24:49'),
(756, 'Action', 'The process of doing something to achieve an aim.', 'Behavior', '2025-11-02 11:24:49'),
(757, 'Result', 'A consequence or outcome of an action.', 'General', '2025-11-02 11:24:49'),
(758, 'Change', 'To make or become different.', 'General', '2025-11-02 11:24:49'),
(759, 'Growth', 'The process of increasing in size, number, or importance.', 'Self-Development', '2025-11-02 11:24:49'),
(760, 'Future', 'The time yet to come.', 'General', '2025-11-02 11:24:49'),
(871, 'Syntax', 'The arrangement of words and phrases to create well-formed sentences.', 'Linguistics', '2025-11-02 11:26:45'),
(872, 'Equity', 'The quality of being fair and impartial.', 'Ethics', '2025-11-02 11:26:45'),
(873, 'Fusion', 'The process of joining two or more things to form a single entity.', 'Science', '2025-11-02 11:26:45'),
(874, 'Species', 'A group of living organisms consisting of similar individuals.', 'Biology', '2025-11-02 11:26:45'),
(875, 'Erosion', 'The gradual destruction or diminution of something by natural forces.', 'Geology', '2025-11-02 11:26:45'),
(876, 'Magnetism', 'A physical phenomenon produced by the motion of electric charge.', 'Physics', '2025-11-02 11:26:45'),
(877, 'Frequency', 'The rate at which a repeated event occurs.', 'Science', '2025-11-02 11:26:45'),
(878, 'Quantum', 'A discrete quantity of energy proportional to the frequency of radiation.', 'Physics', '2025-11-02 11:26:45'),
(879, 'Nebula', 'A cloud of gas and dust in outer space, visible in the night sky.', 'Astronomy', '2025-11-02 11:26:45'),
(880, 'Harbor', 'A place on the coast where ships can find shelter.', 'Geography', '2025-11-02 11:26:45'),
(881, 'Currency', 'A system of money in general use in a country.', 'Economics', '2025-11-02 11:26:45'),
(882, 'Harvest', 'The process or period of gathering crops.', 'Agriculture', '2025-11-02 11:26:45'),
(883, 'Throne', 'A ceremonial chair for a sovereign.', 'History', '2025-11-02 11:26:45'),
(884, 'Empire', 'An extensive group of states or countries ruled over by a single authority.', 'History', '2025-11-02 11:26:45'),
(885, 'Democracy', 'A system of government by the whole population or eligible members.', 'Politics', '2025-11-02 11:26:45'),
(886, 'Senate', 'An assembly or council of citizens having the highest deliberative functions.', 'Politics', '2025-11-02 11:26:45'),
(887, 'Census', 'An official count or survey of a population.', 'Government', '2025-11-02 11:26:45'),
(888, 'Treaty', 'A formally concluded agreement between states.', 'Politics', '2025-11-02 11:26:45'),
(889, 'Harsh', 'Unpleasantly rough or jarring to the senses.', 'General', '2025-11-02 11:26:45'),
(890, 'Serenity', 'The state of being calm, peaceful, and untroubled.', 'Emotion', '2025-11-02 11:26:45'),
(891, 'Eclipse', 'An event where one celestial body moves into the shadow of another.', 'Astronomy', '2025-11-02 11:26:45'),
(892, 'Vortex', 'A mass of whirling fluid or air.', 'Science', '2025-11-02 11:26:45'),
(893, 'Resonance', 'The reinforcement of sound by vibration.', 'Physics', '2025-11-02 11:26:45'),
(894, 'Catalyst', 'A substance that increases the rate of a chemical reaction.', 'Chemistry', '2025-11-02 11:26:45'),
(895, 'Glacier', 'A slowly moving mass of ice.', 'Geography', '2025-11-02 11:26:45'),
(896, 'Habitat', 'The natural home of an organism.', 'Biology', '2025-11-02 11:26:45'),
(897, 'Tundra', 'A flat, treeless Arctic region with permafrost.', 'Geography', '2025-11-02 11:26:45'),
(898, 'Archive', 'A collection of historical documents or records.', 'History', '2025-11-02 11:26:45'),
(899, 'Dynasty', 'A line of hereditary rulers of a country.', 'History', '2025-11-02 11:26:45'),
(900, 'Artifact', 'An object made by a human being, typically of cultural interest.', 'Archaeology', '2025-11-02 11:26:45'),
(901, 'Ceremony', 'A formal religious or public occasion.', 'Culture', '2025-11-02 11:26:45'),
(902, 'Festival', 'A day or period of celebration, typically for religious reasons.', 'Culture', '2025-11-02 11:26:45'),
(903, 'Symmetry', 'The quality of being made up of identical parts facing each other.', 'Mathematics', '2025-11-02 11:26:45'),
(904, 'Variable', 'An element that can change within a set of conditions.', 'Mathematics', '2025-11-02 11:26:45'),
(905, 'Ratio', 'The quantitative relation between two amounts.', 'Mathematics', '2025-11-02 11:26:45'),
(906, 'Radius', 'A straight line from the center to the circumference of a circle.', 'Mathematics', '2025-11-02 11:26:45'),
(907, 'Element', 'A substance that cannot be broken down into simpler substances.', 'Chemistry', '2025-11-02 11:26:45'),
(908, 'Protein', 'A molecule made up of amino acids; essential for living organisms.', 'Biology', '2025-11-02 11:26:45'),
(909, 'Neuron', 'A specialized cell transmitting nerve impulses.', 'Biology', '2025-11-02 11:26:45'),
(910, 'Database', 'An organized collection of structured information or data.', 'Technology', '2025-11-02 11:26:45'),
(911, 'Firewall', 'A security system designed to prevent unauthorized access.', 'Technology', '2025-11-02 11:26:45'),
(912, 'Bandwidth', 'The range of frequencies within a transmission channel.', 'Technology', '2025-11-02 11:26:45'),
(913, 'Satellite', 'An artificial body placed in orbit around the earth or moon.', 'Astronomy', '2025-11-02 11:26:45'),
(914, 'Meteor', 'A small body of matter from outer space that enters the atmosphere.', 'Astronomy', '2025-11-02 11:26:45'),
(915, 'Climate', 'The weather conditions prevailing in an area over a long period.', 'Geography', '2025-11-02 11:26:45'),
(916, 'Season', 'Each of the four divisions of the year.', 'Geography', '2025-11-02 11:26:45'),
(917, 'Horizon', 'The line at which the earth’s surface and the sky appear to meet.', 'Geography', '2025-11-02 11:26:45'),
(918, 'Continent', 'One of the large landmasses of the earth.', 'Geography', '2025-11-02 11:26:45'),
(919, 'Island', 'A piece of land surrounded by water.', 'Geography', '2025-11-02 11:26:45'),
(920, 'Canyon', 'A deep gorge with a river flowing through it.', 'Geography', '2025-11-02 11:26:45'),
(921, 'Jungle', 'An area of dense forest and tangled vegetation.', 'Nature', '2025-11-02 11:26:45'),
(922, 'Volcano', 'A mountain that can erupt with lava and gas.', 'Geology', '2025-11-02 11:26:45'),
(923, 'Crystal', 'A solid material with a natural geometric structure.', 'Science', '2025-11-02 11:26:45'),
(924, 'Particle', 'A minute portion of matter.', 'Physics', '2025-11-02 11:26:45'),
(925, 'Spectrum', 'A band of colors produced when light is dispersed.', 'Science', '2025-11-02 11:26:45'),
(926, 'Velocity', 'Speed in a given direction.', 'Physics', '2025-11-02 11:26:45'),
(927, 'Reaction', 'A process in which substances interact chemically.', 'Chemistry', '2025-11-02 11:26:45'),
(928, 'Resistance', 'The refusal to accept or comply with something.', 'General', '2025-11-02 11:26:45'),
(929, 'Accent', 'A distinctive mode of pronunciation.', 'Linguistics', '2025-11-02 11:26:45'),
(930, 'Dialogue', 'Conversation between two or more people.', 'Communication', '2025-11-02 11:26:45'),
(931, 'Broadcast', 'Transmit information or programs by radio or television.', 'Media', '2025-11-02 11:26:45'),
(932, 'Cinema', 'The art of making motion pictures.', 'Arts', '2025-11-02 11:26:45'),
(933, 'Canvas', 'A strong cloth used for painting or making sails.', 'Arts', '2025-11-02 11:26:45'),
(934, 'Melody', 'A sequence of single notes that is musically satisfying.', 'Music', '2025-11-02 11:26:45'),
(935, 'Rhythm', 'A strong, regular repeated pattern of sound or movement.', 'Music', '2025-11-02 11:26:45'),
(936, 'Lyric', 'The words of a song.', 'Music', '2025-11-02 11:26:45'),
(937, 'Myth', 'A traditional story explaining natural or social phenomena.', 'Culture', '2025-11-02 11:26:45'),
(938, 'Legend', 'A traditional story sometimes regarded as historical.', 'Culture', '2025-11-02 11:26:45'),
(939, 'Fable', 'A short story conveying a moral.', 'Literature', '2025-11-02 11:26:45'),
(940, 'Prophecy', 'A prediction of what will happen in the future.', 'Religion', '2025-11-02 11:26:45'),
(941, 'Temple', 'A building devoted to the worship of a god or gods.', 'Religion', '2025-11-02 11:26:45'),
(942, 'Blessing', 'God’s favor and protection.', 'Religion', '2025-11-02 11:26:45'),
(943, 'Miracle', 'An extraordinary event manifesting divine intervention.', 'Religion', '2025-11-02 11:26:45'),
(967, 'Function', 'A block of organized, reusable code used to perform a single action.', 'Coding', '2025-11-02 11:28:09'),
(968, 'Loop', 'A sequence of instructions that repeats until a certain condition is met.', 'Coding', '2025-11-02 11:28:09'),
(969, 'Compiler', 'A program that converts source code into executable machine code.', 'Coding', '2025-11-02 11:28:09'),
(970, 'Array', 'An ordered collection of elements, each identified by an index.', 'Coding', '2025-11-02 11:28:09'),
(971, 'Boolean', 'A data type that has only two possible values: true or false.', 'Coding', '2025-11-02 11:28:09'),
(972, 'Class', 'A blueprint for creating objects in object-oriented programming.', 'Coding', '2025-11-02 11:28:09'),
(973, 'Object', 'An instance of a class containing data and methods.', 'Coding', '2025-11-02 11:28:09'),
(974, 'Parameter', 'A variable used to pass information into a function.', 'Coding', '2025-11-02 11:28:09'),
(975, 'Integer', 'A whole number; not a fraction.', 'Mathematics', '2025-11-02 11:28:09'),
(976, 'Fraction', 'A numerical quantity that is not a whole number.', 'Mathematics', '2025-11-02 11:28:09'),
(977, 'Exponent', 'A number that shows how many times a base is multiplied by itself.', 'Mathematics', '2025-11-02 11:28:09'),
(978, 'Geometry', 'The branch of mathematics concerned with shapes, sizes, and properties of space.', 'Mathematics', '2025-11-02 11:28:09'),
(979, 'Algebra', 'The part of mathematics in which letters and symbols represent numbers.', 'Mathematics', '2025-11-02 11:28:09'),
(980, 'Derivative', 'A measure of how a function changes as its input changes.', 'Mathematics', '2025-11-02 11:28:09'),
(981, 'Formula', 'A concise way of expressing information symbolically.', 'Mathematics', '2025-11-02 11:28:09'),
(982, 'Vector', 'A quantity with both magnitude and direction.', 'Physics', '2025-11-02 11:28:09'),
(983, 'Force', 'An influence that can change the motion of an object.', 'Physics', '2025-11-02 11:28:09'),
(984, 'Molecule', 'Two or more atoms bonded together.', 'Science', '2025-11-02 11:28:09'),
(985, 'Photosynthesis', 'The process by which plants use sunlight to make food.', 'Biology', '2025-11-02 11:28:09'),
(986, 'Virus', 'A tiny infectious agent that can only reproduce inside a host.', 'Biology', '2025-11-02 11:28:09'),
(987, 'Bacteria', 'Single-celled microorganisms that can be beneficial or harmful.', 'Biology', '2025-11-02 11:28:09'),
(988, 'Chlorophyll', 'The green pigment in plants that absorbs light for photosynthesis.', 'Biology', '2025-11-02 11:28:09'),
(989, 'Compound', 'A substance formed when two or more elements combine chemically.', 'Chemistry', '2025-11-02 11:28:09'),
(990, 'Acid', 'A substance that donates hydrogen ions in solution.', 'Chemistry', '2025-11-02 11:28:09'),
(991, 'Base', 'A substance that accepts hydrogen ions in solution.', 'Chemistry', '2025-11-02 11:28:09'),
(992, 'Solution', 'A homogeneous mixture of two or more substances.', 'Chemistry', '2025-11-02 11:28:09'),
(993, 'Reactant', 'A substance that undergoes change in a chemical reaction.', 'Chemistry', '2025-11-02 11:28:09'),
(994, 'Oxidation', 'The process of losing electrons during a reaction.', 'Chemistry', '2025-11-02 11:28:09'),
(995, 'Reduction', 'The process of gaining electrons during a reaction.', 'Chemistry', '2025-11-02 11:28:09'),
(996, 'Circuit', 'A complete path through which electric current flows.', 'Physics', '2025-11-02 11:28:09'),
(997, 'Resistor', 'A device that limits the flow of electrical current.', 'Electronics', '2025-11-02 11:28:09'),
(998, 'Capacitor', 'A device that stores electrical energy in an electric field.', 'Electronics', '2025-11-02 11:28:09'),
(999, 'Transistor', 'A semiconductor device used to amplify or switch signals.', 'Electronics', '2025-11-02 11:28:09'),
(1000, 'Sensor', 'A device that detects or measures a physical property.', 'Technology', '2025-11-02 11:28:09'),
(1001, 'Resolution', 'The amount of detail an image holds.', 'Technology', '2025-11-02 11:28:09'),
(1002, 'Interface', 'A shared boundary where two systems communicate.', 'Technology', '2025-11-02 11:28:09'),
(1003, 'Protocol', 'A set of rules for data exchange between devices.', 'Networking', '2025-11-02 11:28:09'),
(1004, 'Server', 'A computer that provides data or services to other computers.', 'Networking', '2025-11-02 11:28:09'),
(1005, 'Client', 'A device or program that accesses services from a server.', 'Networking', '2025-11-02 11:28:09'),
(1006, 'Query', 'A request for information from a database.', 'Technology', '2025-11-02 11:28:09'),
(1007, 'Encryption', 'The process of converting data into a coded form to prevent unauthorized access.', 'Cybersecurity', '2025-11-02 11:28:09'),
(1008, 'Decryption', 'The process of converting coded data back into readable form.', 'Cybersecurity', '2025-11-02 11:28:09'),
(1009, 'Malware', 'Software designed to disrupt or damage a computer system.', 'Cybersecurity', '2025-11-02 11:28:09'),
(1010, 'Backup', 'A copy of data stored separately for recovery purposes.', 'Technology', '2025-11-02 11:28:09'),
(1011, 'Module', 'A self-contained unit of code that performs a specific function.', 'Coding', '2025-11-02 11:28:09'),
(1012, 'Library', 'A collection of pre-written code used to develop software.', 'Coding', '2025-11-02 11:28:09'),
(1013, 'Framework', 'A platform for developing software applications.', 'Coding', '2025-11-02 11:28:09'),
(1014, 'API', 'A set of functions and procedures allowing applications to access external services.', 'Coding', '2025-11-02 11:28:09'),
(1015, 'Inheritance', 'A mechanism where a class derives properties from another.', 'Coding', '2025-11-02 11:28:09'),
(1016, 'Polymorphism', 'The ability of different objects to respond to the same function in different ways.', 'Coding', '2025-11-02 11:28:09'),
(1017, 'Encapsulation', 'The bundling of data with the methods that operate on that data.', 'Coding', '2025-11-02 11:28:09'),
(1018, 'Debugger', 'A tool used to test and fix errors in code.', 'Coding', '2025-11-02 11:28:09'),
(1019, 'Terminal', 'An interface for typing and executing text-based commands.', 'Technology', '2025-11-02 11:28:09'),
(1020, 'Command', 'An instruction given to a computer to perform an action.', 'Technology', '2025-11-02 11:28:09'),
(1021, 'Script', 'A file containing commands executed by a program or interpreter.', 'Coding', '2025-11-02 11:28:09'),
(1022, 'Constant', 'A value that does not change.', 'Mathematics', '2025-11-02 11:28:09'),
(1023, 'Theorem', 'A statement that has been proven based on previously established statements.', 'Mathematics', '2025-11-02 11:28:09'),
(1024, 'Proof', 'Logical reasoning demonstrating that a statement is true.', 'Mathematics', '2025-11-02 11:28:09'),
(1025, 'Postulate', 'A statement accepted as true without proof.', 'Mathematics', '2025-11-02 11:28:09'),
(1026, 'Model', 'A simplified representation of a system or concept.', 'Science', '2025-11-02 11:28:09'),
(1027, 'Observation', 'Careful watching or monitoring of phenomena.', 'Science', '2025-11-02 11:28:09'),
(1028, 'Hypothesis', 'A proposed explanation based on limited evidence.', 'Science', '2025-11-02 11:28:09'),
(1029, 'Theory', 'A system of ideas intended to explain something.', 'Science', '2025-11-02 11:28:09'),
(1030, 'Evidence', 'Information indicating whether a belief or proposition is true.', 'Science', '2025-11-02 11:28:09'),
(1031, 'Analysis', 'Detailed examination of elements or structure.', 'General', '2025-11-02 11:28:09'),
(1032, 'Conclusion', 'A judgment or decision reached after reasoning.', 'General', '2025-11-02 11:28:09'),
(1033, 'Definition', 'A statement of the meaning of a word or phrase.', 'English', '2025-11-02 11:28:09'),
(1034, 'Sentence', 'A set of words that expresses a complete thought.', 'English', '2025-11-02 11:28:09'),
(1035, 'Paragraph', 'A section of writing dealing with one topic.', 'English', '2025-11-02 11:28:09'),
(1036, 'Grammar', 'The rules of a language’s structure.', 'English', '2025-11-02 11:28:09'),
(1037, 'Adjective', 'A word that describes a noun.', 'English', '2025-11-02 11:28:09'),
(1038, 'Verb', 'A word used to describe an action.', 'English', '2025-11-02 11:28:09'),
(1039, 'Noun', 'A word used to identify people, places, or things.', 'English', '2025-11-02 11:28:09'),
(1040, 'Pronoun', 'A word that replaces a noun.', 'English', '2025-11-02 11:28:09'),
(1041, 'Adverb', 'A word that modifies a verb, adjective, or other adverb.', 'English', '2025-11-02 11:28:09'),
(1042, 'Preposition', 'A word showing relation between a noun and another word.', 'English', '2025-11-02 11:28:09'),
(1066, 'Integral', 'A mathematical concept representing area under a curve.', 'Mathematics', '2025-11-02 11:30:04'),
(1067, 'Evolution', 'The process through which species change over time.', 'Science', '2025-11-02 11:30:04'),
(1068, 'Cell', 'The smallest structural and functional unit of living organisms.', 'Science', '2025-11-02 11:30:04'),
(1069, 'Ecosystem', 'A community of living organisms interacting with their environment.', 'Science', '2025-11-02 11:30:04'),
(1070, 'Bug', 'An error or flaw in a software program.', 'Coding', '2025-11-02 11:30:04'),
(1071, 'Debugging', 'The process of finding and fixing software errors.', 'Coding', '2025-11-02 11:30:04'),
(1072, 'Binary', 'A number system using only two digits, 0 and 1.', 'Technology', '2025-11-02 11:30:04'),
(1073, 'Forest', 'A large area covered chiefly with trees and undergrowth.', 'Environment', '2025-11-02 11:30:04'),
(1074, 'Pollution', 'The presence of harmful substances in the environment.', 'Environment', '2025-11-02 11:30:04'),
(1075, 'Conservation', 'The protection and preservation of natural resources.', 'Environment', '2025-11-02 11:30:04'),
(1076, 'Recycling', 'The process of converting waste into reusable material.', 'Environment', '2025-11-02 11:30:04'),
(1077, 'Biodiversity', 'The variety of life in the world or in a particular habitat.', 'Environment', '2025-11-02 11:30:04'),
(1078, 'Sustainability', 'Meeting present needs without compromising future generations.', 'Environment', '2025-11-02 11:30:04'),
(1079, 'Curriculum', 'The subjects comprising a course of study.', 'Education', '2025-11-02 11:30:04'),
(1080, 'Lecture', 'An educational talk to an audience or class.', 'Education', '2025-11-02 11:30:04'),
(1081, 'Exam', 'A formal test of knowledge or skill.', 'Education', '2025-11-02 11:30:04'),
(1082, 'Scholarship', 'A grant or payment made to support a student’s education.', 'Education', '2025-11-02 11:30:04'),
(1083, 'Assignment', 'A task given as part of studies.', 'Education', '2025-11-02 11:30:04'),
(1084, 'Mentor', 'An experienced person who advises or trains others.', 'Education', '2025-11-02 11:30:04'),
(1085, 'Graduation', 'The completion of a course of study.', 'Education', '2025-11-02 11:30:04'),
(1086, 'Research', 'The systematic investigation to discover facts.', 'Education', '2025-11-02 11:30:04'),
(1087, 'Thesis', 'A long essay involving personal research.', 'Education', '2025-11-02 11:30:04'),
(1088, 'Economy', 'The wealth and resources of a region.', 'Social Studies', '2025-11-02 11:30:04'),
(1089, 'Community', 'A group of people living in the same place.', 'Social Studies', '2025-11-02 11:30:04'),
(1090, 'Globalization', 'The process by which businesses or other organizations develop international influence.', 'Social Studies', '2025-11-02 11:30:04'),
(1091, 'Equality', 'The state of being equal, especially in rights or status.', 'Social Studies', '2025-11-02 11:30:04'),
(1092, 'Government', 'The system by which a nation or state is controlled.', 'Social Studies', '2025-11-02 11:30:04'),
(1093, 'Conflict', 'A serious disagreement or argument.', 'Social Studies', '2025-11-02 11:30:04'),
(1094, 'Friction', 'The resistance that one surface encounters when moving over another.', 'Physics', '2025-11-02 11:30:04'),
(1095, 'Density', 'The mass per unit volume of a substance.', 'Physics', '2025-11-02 11:30:04'),
(1096, 'Acceleration', 'The rate of change of velocity per unit of time.', 'Physics', '2025-11-02 11:30:04'),
(1097, 'Perception', 'The ability to see, hear, or become aware of something.', 'Psychology', '2025-11-02 11:30:04'),
(1098, 'Cognition', 'The mental action of acquiring knowledge and understanding.', 'Psychology', '2025-11-02 11:30:04'),
(1099, 'Behavior', 'The way one acts or conducts oneself.', 'Psychology', '2025-11-02 11:30:04'),
(1100, 'Personality', 'The combination of characteristics that form an individual’s character.', 'Psychology', '2025-11-02 11:30:04'),
(1101, 'Stress', 'A state of mental or emotional strain or tension.', 'Psychology', '2025-11-02 11:30:04'),
(1165, 'Clock', 'A device for measuring and showing the time.', 'Everyday', '2025-11-02 11:31:18'),
(1166, 'Mirror', 'A reflective surface that forms an image of an object.', 'Everyday', '2025-11-02 11:31:18'),
(1167, 'Bag', 'A flexible container used for carrying things.', 'Everyday', '2025-11-02 11:31:18'),
(1168, 'Bottle', 'A container with a narrow neck used for liquids.', 'Everyday', '2025-11-02 11:31:18'),
(1169, 'Speak', 'To use the voice to express words.', 'Action', '2025-11-02 11:31:18'),
(1170, 'Happy', 'Feeling or showing pleasure or contentment.', 'Emotion', '2025-11-02 11:31:18');
INSERT INTO `dictionary` (`id`, `word`, `definition`, `category`, `created_at`) VALUES
(1171, 'Sad', 'Feeling or showing sorrow or unhappiness.', 'Emotion', '2025-11-02 11:31:18'),
(1172, 'Angry', 'Feeling or showing strong annoyance or displeasure.', 'Emotion', '2025-11-02 11:31:18'),
(1173, 'Afraid', 'Feeling fear or anxiety.', 'Emotion', '2025-11-02 11:31:18'),
(1174, 'Excited', 'Very enthusiastic and eager.', 'Emotion', '2025-11-02 11:31:18'),
(1175, 'Tired', 'Needing rest or sleep.', 'Emotion', '2025-11-02 11:31:18'),
(1176, 'Bored', 'Feeling weary because of lack of interest.', 'Emotion', '2025-11-02 11:31:18'),
(1177, 'Calm', 'Not showing or feeling nervousness or anger.', 'Emotion', '2025-11-02 11:31:18'),
(1178, 'Proud', 'Feeling deep satisfaction from achievements.', 'Emotion', '2025-11-02 11:31:18'),
(1179, 'Worried', 'Anxious or troubled about actual or potential problems.', 'Emotion', '2025-11-02 11:31:18'),
(1180, 'Beach', 'A shore covered with sand or small stones.', 'Nature', '2025-11-02 11:31:18'),
(1181, 'Problem', 'A matter regarded as needing to be dealt with.', 'Concept', '2025-11-02 11:31:18'),
(1182, 'Reason', 'A cause, explanation, or justification for an action.', 'Concept', '2025-11-02 11:31:18'),
(1183, 'Pencil', 'An instrument for writing or drawing, using graphite.', 'Education', '2025-11-02 11:31:18'),
(1184, 'Lesson', 'A period of learning or teaching.', 'Education', '2025-11-02 11:31:18'),
(1185, 'Subject', 'An area of knowledge studied in school.', 'Education', '2025-11-02 11:31:18'),
(1186, 'Test', 'An examination to measure knowledge or ability.', 'Education', '2025-11-02 11:31:18'),
(1187, 'Grade', 'A mark or level of quality in a test or course.', 'Education', '2025-11-02 11:31:18'),
(1188, 'Store', 'A place where things are kept or sold.', 'Everyday', '2025-11-02 11:31:18'),
(1189, 'Job', 'A paid position of employment.', 'Everyday', '2025-11-02 11:31:18'),
(1190, 'Street', 'A public road in a city or town.', 'Everyday', '2025-11-02 11:31:18'),
(1191, 'File', 'A collection of data stored together in a computer.', 'Technology', '2025-11-02 11:31:18'),
(1192, 'Folder', 'A container used to organize files.', 'Technology', '2025-11-02 11:31:18'),
(1193, 'Website', 'A collection of web pages accessible on the Internet.', 'Technology', '2025-11-02 11:31:18'),
(1194, 'Password', 'A secret word used to gain access to a system.', 'Technology', '2025-11-02 11:31:18'),
(1245, 'I', 'Used by a speaker to refer to themselves.', 'Grammar', '2025-11-02 11:32:21'),
(1246, 'You', 'Used to refer to the person or people being spoken to.', 'Grammar', '2025-11-02 11:32:21'),
(1247, 'He', 'Used to refer to a male person or animal previously mentioned.', 'Grammar', '2025-11-02 11:32:21'),
(1248, 'She', 'Used to refer to a female person or animal previously mentioned.', 'Grammar', '2025-11-02 11:32:21'),
(1249, 'It', 'Used to refer to a thing, animal, or idea already mentioned.', 'Grammar', '2025-11-02 11:32:21'),
(1250, 'We', 'Used by a speaker to refer to themselves and one or more other people.', 'Grammar', '2025-11-02 11:32:21'),
(1251, 'They', 'Used to refer to two or more people or things previously mentioned.', 'Grammar', '2025-11-02 11:32:21'),
(1252, 'Me', 'Used by a speaker to refer to themselves as the object of a verb or preposition.', 'Grammar', '2025-11-02 11:32:21'),
(1253, 'Him', 'Used to refer to a male person as the object of a verb or preposition.', 'Grammar', '2025-11-02 11:32:21'),
(1254, 'Her', 'Used to refer to a female person as the object of a verb or preposition.', 'Grammar', '2025-11-02 11:32:21'),
(1255, 'This', 'Used to identify a specific person or thing close at hand.', 'Grammar', '2025-11-02 11:32:21'),
(1256, 'That', 'Used to identify a specific person or thing not close at hand.', 'Grammar', '2025-11-02 11:32:21'),
(1257, 'These', 'Used to refer to more than one thing close at hand.', 'Grammar', '2025-11-02 11:32:21'),
(1258, 'Those', 'Used to refer to more than one thing not close at hand.', 'Grammar', '2025-11-02 11:32:21'),
(1259, 'Who', 'Used to ask or talk about what person or people.', 'Grammar', '2025-11-02 11:32:21'),
(1260, 'What', 'Used to ask for information about something.', 'Grammar', '2025-11-02 11:32:21'),
(1261, 'Where', 'Used to ask about a place or position.', 'Grammar', '2025-11-02 11:32:21'),
(1262, 'When', 'Used to ask about time.', 'Grammar', '2025-11-02 11:32:21'),
(1263, 'Why', 'Used to ask for a reason or explanation.', 'Grammar', '2025-11-02 11:32:21'),
(1264, 'How', 'Used to ask about the way or manner in which something is done.', 'Grammar', '2025-11-02 11:32:21'),
(1265, 'Am', 'Present tense of \"be\" used with I.', 'Grammar', '2025-11-02 11:32:21'),
(1266, 'Is', 'Present tense of \"be\" used with he, she, or it.', 'Grammar', '2025-11-02 11:32:21'),
(1267, 'Are', 'Present tense of \"be\" used with you, we, or they.', 'Grammar', '2025-11-02 11:32:21'),
(1268, 'Was', 'Past tense of \"be\" used with I, he, she, or it.', 'Grammar', '2025-11-02 11:32:21'),
(1269, 'Were', 'Past tense of \"be\" used with you, we, or they.', 'Grammar', '2025-11-02 11:32:21'),
(1270, 'Be', 'The base form of the verb \"to be\".', 'Grammar', '2025-11-02 11:32:21'),
(1271, 'Been', 'Past participle of \"be\".', 'Grammar', '2025-11-02 11:32:21'),
(1272, 'Being', 'Present participle of \"be\".', 'Grammar', '2025-11-02 11:32:21'),
(1273, 'Do', 'Used to form questions or negatives; also means to perform an action.', 'Grammar', '2025-11-02 11:32:21'),
(1274, 'Does', 'Third person singular form of \"do\".', 'Grammar', '2025-11-02 11:32:21'),
(1275, 'Did', 'Past tense of \"do\".', 'Grammar', '2025-11-02 11:32:21'),
(1276, 'Have', 'To possess, own, or hold something.', 'Grammar', '2025-11-02 11:32:21'),
(1277, 'Has', 'Third person singular form of \"have\".', 'Grammar', '2025-11-02 11:32:21'),
(1278, 'Had', 'Past tense of \"have\".', 'Grammar', '2025-11-02 11:32:21'),
(1279, 'Can', 'Used to indicate ability or permission.', 'Grammar', '2025-11-02 11:32:21'),
(1280, 'Could', 'Past tense of \"can\"; also expresses possibility or permission.', 'Grammar', '2025-11-02 11:32:21'),
(1281, 'Will', 'Used to express future actions or willingness.', 'Grammar', '2025-11-02 11:32:21'),
(1282, 'Would', 'Used to express a polite request or hypothetical situation.', 'Grammar', '2025-11-02 11:32:21'),
(1283, 'Shall', 'Used to express future actions or obligations (formal).', 'Grammar', '2025-11-02 11:32:21'),
(1284, 'Should', 'Used to indicate duty, correctness, or a recommendation.', 'Grammar', '2025-11-02 11:32:21'),
(1285, 'May', 'Used to express permission or possibility.', 'Grammar', '2025-11-02 11:32:21'),
(1286, 'Might', 'Used to express a smaller possibility.', 'Grammar', '2025-11-02 11:32:21'),
(1287, 'Must', 'Used to express necessity or strong obligation.', 'Grammar', '2025-11-02 11:32:21'),
(1288, 'In', 'Used to indicate location or position within something.', 'Grammar', '2025-11-02 11:32:21'),
(1289, 'On', 'Used to indicate something resting or attached to a surface.', 'Grammar', '2025-11-02 11:32:21'),
(1290, 'At', 'Used to indicate a specific place or time.', 'Grammar', '2025-11-02 11:32:21'),
(1291, 'By', 'Used to indicate the means or method of doing something.', 'Grammar', '2025-11-02 11:32:21'),
(1292, 'From', 'Used to indicate the starting point of motion or time.', 'Grammar', '2025-11-02 11:32:21'),
(1293, 'With', 'Used to indicate being together or having something.', 'Grammar', '2025-11-02 11:32:21'),
(1294, 'To', 'Used to indicate direction, place, or position.', 'Grammar', '2025-11-02 11:32:21'),
(1295, 'Of', 'Used to show belonging or connection.', 'Grammar', '2025-11-02 11:32:21'),
(1296, 'For', 'Used to show purpose, use, or intended recipient.', 'Grammar', '2025-11-02 11:32:21'),
(1297, 'About', 'Used to indicate the subject of something.', 'Grammar', '2025-11-02 11:32:21'),
(1298, 'And', 'Used to connect words of the same part of speech or ideas.', 'Grammar', '2025-11-02 11:32:21'),
(1299, 'But', 'Used to introduce contrast or exception.', 'Grammar', '2025-11-02 11:32:21'),
(1300, 'Or', 'Used to present an alternative or choice.', 'Grammar', '2025-11-02 11:32:21'),
(1301, 'So', 'Used to show result, reason, or purpose.', 'Grammar', '2025-11-02 11:32:21'),
(1302, 'Because', 'Used to give a reason or cause.', 'Grammar', '2025-11-02 11:32:21'),
(1303, 'If', 'Used to express a condition or possibility.', 'Grammar', '2025-11-02 11:32:21'),
(1304, 'Then', 'Used to show what happens next in time or order.', 'Grammar', '2025-11-02 11:32:21'),
(1305, 'Than', 'Used for comparison.', 'Grammar', '2025-11-02 11:32:21'),
(1306, 'Though', 'Used to introduce contrast or exception.', 'Grammar', '2025-11-02 11:32:21'),
(1307, 'While', 'Used to indicate that two actions happen at the same time.', 'Grammar', '2025-11-02 11:32:21');

-- --------------------------------------------------------

--
-- Table structure for table `dictionary_requests`
--

CREATE TABLE `dictionary_requests` (
  `id` int(11) NOT NULL,
  `word` varchar(255) NOT NULL,
  `definition` text NOT NULL,
  `category` varchar(100) DEFAULT 'General',
  `requested_by` varchar(100) NOT NULL,
  `status` enum('pending','approved','rejected') DEFAULT 'pending',
  `reviewed_by` varchar(100) DEFAULT NULL,
  `requested_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `reviewed_at` timestamp NULL DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Dumping data for table `dictionary_requests`
--

INSERT INTO `dictionary_requests` (`id`, `word`, `definition`, `category`, `requested_by`, `status`, `reviewed_by`, `requested_at`, `reviewed_at`) VALUES
(1, 'Euwan', 'An awesome person', 'General', 'eee', 'pending', NULL, '2025-11-02 10:15:38', NULL),
(2, 'Cone', 'cool stuff', 'General', 'eee', 'pending', NULL, '2025-11-02 10:45:24', NULL),
(3, 'Epic', 'Outplay', 'General', 'eee', 'pending', NULL, '2025-11-02 10:45:37', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `favorites`
--

CREATE TABLE `favorites` (
  `id` int(11) NOT NULL,
  `username` varchar(100) NOT NULL,
  `word` varchar(255) NOT NULL,
  `favorited_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Dumping data for table `favorites`
--

INSERT INTO `favorites` (`id`, `username`, `word`, `favorited_at`) VALUES
(1, 'Admin', 'Achievement', '2025-11-02 11:06:26');

-- --------------------------------------------------------

--
-- Table structure for table `profiles`
--

CREATE TABLE `profiles` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `full_name` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `bio` text DEFAULT NULL,
  `avatar_color` varchar(50) DEFAULT '#DC4600',
  `updated_at` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `profiles`
--

INSERT INTO `profiles` (`id`, `user_id`, `full_name`, `email`, `bio`, `avatar_color`, `updated_at`) VALUES
(1, 2, 'eee', '', 'cool', '#DC4600', '2025-11-01 13:42:53'),
(2, 5, 'admin', '', NULL, '#DC4600', '2025-11-02 18:14:38');

-- --------------------------------------------------------

--
-- Table structure for table `schedules`
--

CREATE TABLE `schedules` (
  `id` int(11) NOT NULL,
  `status` tinyint(1) DEFAULT 0,
  `date` varchar(50) DEFAULT NULL,
  `time` varchar(50) DEFAULT NULL,
  `priority` varchar(20) DEFAULT NULL,
  `category` varchar(100) DEFAULT NULL,
  `notes` text DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `schedules`
--

INSERT INTO `schedules` (`id`, `status`, `date`, `time`, `priority`, `category`, `notes`, `created_at`) VALUES
(1, 0, '11/14/2025', '03:00', 'Medium', 'Work', 'Final Defense', '2025-11-07 08:26:41');

-- --------------------------------------------------------

--
-- Table structure for table `text`
--

CREATE TABLE `text` (
  `id` int(11) NOT NULL,
  `tab_name` varchar(255) NOT NULL,
  `content` longtext DEFAULT NULL,
  `theme_index` int(11) DEFAULT 0,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `text`
--

INSERT INTO `text` (`id`, `tab_name`, `content`, `theme_index`, `created_at`, `updated_at`) VALUES
(1, '1', '{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Tempus Sans ITC;}{\\f1\\fnil Tempus Sans ITC;}{\\f2\\fnil\\fcharset0 Tw Cen MT;}{\\f3\\fnil\\fcharset0 Wide Latin;}}\r\n{\\colortbl ;\\red60\\green40\\blue20;}\r\n{\\*\\generator Riched20 10.0.26100}\\viewkind4\\uc1 \r\n\\pard\\cf1\\f0\\fs23 dwas dawsd dwasdwd dwdadasdw dawd dd2\\f1\\par\r\n\\par\r\n\\par\r\n\\par\r\n\\f0 dawadaw\\par\r\nddwdawdaw\\par\r\n\\f2 dwasd\\par\r\n\\par\r\n\\f3 dwasdwasdwa\\f0\\par\r\n\\par\r\ndawda\\par\r\n\\par\r\n\\par\r\n\\par\r\n\\par\r\nawda\\par\r\n\\par\r\n\\par\r\n\\par\r\ndada\\par\r\n\\par\r\n\\par\r\n\\par\r\nwdaa\\par\r\n\\par\r\n\\par\r\n\\par\r\nwadw\\par\r\n\\par\r\ndawdw\\f1\\par\r\n}\r\n', 7, '2025-11-10 14:37:55', '2025-11-10 14:38:13'),
(2, 'Wersd', '{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Sitka Banner;}{\\f1\\fnil\\fcharset0 Viner Hand ITC;}{\\f2\\fnil\\fcharset0 Sitka Heading Semibold;}{\\f3\\fnil Sitka Banner;}}\r\n{\\colortbl ;\\red80\\green40\\blue0;\\red255\\green255\\blue255;}\r\n{\\*\\generator Riched20 10.0.26100}\\viewkind4\\uc1 \r\n\\pard\\cf1\\highlight2\\f0\\fs23 co\\f1 ol\\f0 dd\\par\r\n\\par\r\nw d\\fs56 awd\\fs23 a\\b dw\\b0 ad\\par\r\n\\par\r\nd   d\\f2 wa\\f0 dswasdwa \\fs48 dwadwadasd \\highlight0\\f3\\fs23\\par\r\n}\r\n', 5, '2025-11-10 14:39:17', '2025-11-10 14:39:43'),
(3, 'we', '{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Segoe UI;}{\\f1\\fnil Segoe UI;}}\r\n{\\*\\generator Riched20 10.0.26100}\\viewkind4\\uc1 \r\n\\pard\\f0\\fs23 warr\\f1\\par\r\n}\r\n', 0, '2025-11-10 14:41:19', '2025-11-10 14:41:22');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` int(10) UNSIGNED NOT NULL,
  `username` varchar(50) NOT NULL,
  `email` varchar(255) NOT NULL,
  `password` varchar(250) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `username`, `email`, `password`) VALUES
(1, 'Shidos1', 'Shidos1@gmail.com', 'qwerty1.'),
(2, 'eee', '', 'eee'),
(5, 'Admin', 'e', '123'),
(22, 'Fritz', 'fritz@gmail.com', 'fritez213'),
(23, 'Wewecars', 'wewcars@gmail.com', 'wewe232131');

-- --------------------------------------------------------

--
-- Table structure for table `workspaces`
--

CREATE TABLE `workspaces` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `workspace_name` varchar(255) NOT NULL,
  `file_path` varchar(500) NOT NULL,
  `created_at` datetime NOT NULL,
  `updated_at` datetime NOT NULL,
  `is_favorite` tinyint(1) NOT NULL DEFAULT 0,
  `is_archived` tinyint(1) NOT NULL DEFAULT 0,
  `color_tag` varchar(20) DEFAULT '#DC4600',
  `category` varchar(50) DEFAULT 'Work'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `workspaces`
--

INSERT INTO `workspaces` (`id`, `user_id`, `workspace_name`, `file_path`, `created_at`, `updated_at`, `is_favorite`, `is_archived`, `color_tag`, `category`) VALUES
(1, 1, 'Warp', 'C:\\Users\\Euwan\\Documents\\PlanIT\\Shidos1\\Workspace_20251031_105844.json', '2025-10-31 10:58:44', '2025-10-31 12:35:43', 1, 0, '#DC4600', 'Work'),
(2, 2, 'Awesome', 'C:\\Users\\Euwan\\Documents\\PlanIT\\eee\\Workspace_20251031_144752.json', '2025-10-31 14:47:52', '2025-11-10 22:39:14', 1, 0, '#1ABC9C', 'Work'),
(3, 2, 'Ruth', 'C:\\Users\\Euwan\\Documents\\PlanIT\\eee\\Workspace_20251031_145507.json', '2025-10-31 14:55:07', '2025-10-31 16:50:03', 0, 0, '#DC4600', 'Work'),
(4, 2, 'Sample 1', 'C:\\Users\\Euwan\\Documents\\PlanIT\\eee\\Workspace_20251031_164957.json', '2025-10-31 16:49:57', '2025-11-04 16:37:14', 0, 0, '#DC4600', 'Work'),
(8, 2, 'Workspace_20251031_165011', 'C:\\Users\\Euwan\\Documents\\PlanIT\\eee\\Workspace_20251031_165011.json', '2025-10-31 16:50:11', '2025-10-31 16:50:11', 0, 0, '#DC4600', 'Work'),
(10, 5, 'Admin', 'C:\\Users\\Euwan\\Documents\\PlanIT\\Admin\\Admin.json', '2025-11-02 18:16:26', '2025-11-03 22:12:40', 1, 0, '#DC4600', 'Work'),
(12, 2, 'Workspace_20251104_165315', 'C:\\Users\\Euwan\\Documents\\PlanIT\\eee\\Workspace_20251104_165315.json', '2025-11-04 16:53:16', '2025-11-10 22:41:18', 1, 0, '#9B59B6', 'Personal'),
(13, 2, 'Tabs', 'C:\\Users\\Euwan\\Documents\\PlanIT\\eee\\Tabs.json', '2025-11-07 15:52:54', '2025-11-10 22:40:38', 0, 0, '#2ECC71', 'Business');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `checklists`
--
ALTER TABLE `checklists`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `dictionary`
--
ALTER TABLE `dictionary`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `word` (`word`),
  ADD KEY `idx_word` (`word`),
  ADD KEY `idx_category` (`category`);

--
-- Indexes for table `dictionary_requests`
--
ALTER TABLE `dictionary_requests`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_status` (`status`),
  ADD KEY `idx_requested_by` (`requested_by`);

--
-- Indexes for table `favorites`
--
ALTER TABLE `favorites`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_favorite` (`username`,`word`),
  ADD KEY `idx_username` (`username`),
  ADD KEY `idx_word` (`word`);

--
-- Indexes for table `profiles`
--
ALTER TABLE `profiles`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `user_id` (`user_id`);

--
-- Indexes for table `schedules`
--
ALTER TABLE `schedules`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `text`
--
ALTER TABLE `text`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `username` (`username`),
  ADD UNIQUE KEY `email` (`email`);

--
-- Indexes for table `workspaces`
--
ALTER TABLE `workspaces`
  ADD PRIMARY KEY (`id`),
  ADD KEY `user_id` (`user_id`),
  ADD KEY `idx_user_favorite` (`user_id`,`is_favorite`),
  ADD KEY `idx_user_archived` (`user_id`,`is_archived`),
  ADD KEY `idx_updated` (`updated_at`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `checklists`
--
ALTER TABLE `checklists`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `dictionary`
--
ALTER TABLE `dictionary`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=1308;

--
-- AUTO_INCREMENT for table `dictionary_requests`
--
ALTER TABLE `dictionary_requests`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `favorites`
--
ALTER TABLE `favorites`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `profiles`
--
ALTER TABLE `profiles`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `schedules`
--
ALTER TABLE `schedules`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `text`
--
ALTER TABLE `text`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=24;

--
-- AUTO_INCREMENT for table `workspaces`
--
ALTER TABLE `workspaces`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
