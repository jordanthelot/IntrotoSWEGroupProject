# Digital Wellness Game

A Unity-based platformer designed to encourage players to reflect on their social media habits and digital well-being.

## About the Project

The **Digital Wellness Game** is a team software engineering project that combines gameplay with self-reflection. The game focuses on the effects of excessive social media scrolling and encourages players to think about how their digital habits affect their time, attention, and overall well-being.

Rather than simply telling players to spend less time online, the game uses an interactive platforming experience to present prompts, messages, and situations related to digital wellness.

## Purpose

Social media is a major part of everyday life, but it can be easy to lose track of how much time we spend scrolling. Our goal is to create an engaging experience that encourages players to become more aware of their digital habits.

The project explores topics such as:

- Social media usage and scrolling habits
- Screen-time awareness
- Digital wellness
- Self-reflection
- Healthy technology habits
- Mindful social media use

## Features

The project is being developed with features including:

- **2D Platformer Gameplay** — Players navigate through an interactive Unity environment.
- **Digital Wellness Prompts** — Messages and questions encourage players to reflect on their social media habits.
- **User Accounts** — Members can create accounts and securely access the application.
- **User Profiles** — Players can access information associated with their account and gameplay.
- **Dynamic Content** — Game content can be stored and managed through the application's database.
- **Admin and Member Roles** — The system supports different user types with different permissions.
- **Responsive UI** — Menus and interface elements are designed to provide a clear and accessible user experience.

Additional gameplay and wellness features will be added throughout development.

## Technologies

**Game Engine:** Unity  
**Programming Language:** C#  
**Version Control:** Git & GitHub  
**Project Management:** Scrum / Agile Development  
**Database:** Dynamic database integration  
**Collaboration:** GitHub branches and pull requests

## System Overview

The application follows a layered structure that separates the major responsibilities of the system:

**Frontend / Presentation Layer**
- Unity game interface
- Login and account UI
- Player profile
- Menus
- Gameplay elements
- Digital wellness prompts

**Application / Logic Layer**
- Authentication logic
- Player interactions
- Game state management
- User management
- Digital wellness content logic

**Data Layer**
- User information
- Account information
- Game-related data
- Dynamic digital wellness content

This separation helps make the application easier to develop, maintain, test, and expand.

## Team

This project is being developed as a team project for **CEN3031 – Introduction to Software Engineering at the University of Florida**.

| Team Member | Primary Role |
| --- | --- |
| Amber Nguyen | Product Manager, Scrum Master, Frontend Developer |
| Soni | Full-Stack / Backend Developer |
| Jordan | Backend Developer |
| Raul | Backend Developer |

Team members collaborate across different parts of the application as needed throughout each sprint.

## Development Workflow

The team uses an Agile/Scrum development process.

Development work generally follows this workflow:

1. Select a Product Backlog Item (PBI).
2. Break the PBI into development tasks.
3. Create or work from the appropriate Git branch.
4. Implement and test the feature.
5. Commit changes with a descriptive commit message.
6. Push the branch to GitHub.
7. Create a Pull Request.
8. Review and merge completed work into the main project.

## Running the Project

### Requirements

Before opening the project, make sure you have:

- Unity Hub
- The team's compatible Unity Editor version
- Git or GitHub Desktop

### Setup

Clone the repository:

```bash
git clone https://github.com/jordanthelot/IntrotoSWEGroupProject.git
```

Then:

1. Open **Unity Hub**.
2. Select **Add → Add project from disk**.
3. Navigate to the cloned `IntrotoSWEGroupProject` folder.
4. Select the Unity project folder.
5. Open the project using the appropriate Unity Editor version.
6. Allow Unity to import and generate the required project files.
7. Open the main game scene.
8. Press **Play** in the Unity Editor to run the game.

## Contributing

Before beginning development, pull the latest changes:

```bash
git pull
```

Create a branch for your work when appropriate:

```bash
git checkout -b feature/feature-name
```

After completing your changes:

```bash
git add .
git commit -m "Describe the changes made"
git push
```

Create a Pull Request on GitHub so the changes can be reviewed and merged into the project.

Avoid directly editing another team member's active feature branch unless the team has coordinated the change.

## Project Status

**Currently in Development**

The Digital Wellness Game is actively being developed. Features, gameplay mechanics, UI elements, and backend functionality may change as the team progresses through future sprints.

## Future Development

Potential additions and improvements include:

- Additional digital wellness prompts
- Expanded platforming levels
- Improved UI and visual design
- Player progress tracking
- More personalized reflection experiences
- Additional account and profile functionality
- Expanded database integration
- Accessibility improvements
- Additional feedback based on player choices

## Educational Purpose

This project was created for educational purposes as part of the University of Florida's **CEN3031 – Introduction to Software Engineering** course. It demonstrates software engineering concepts including Agile development, Scrum, product backlog management, requirements engineering, software architecture, version control, collaborative development, frontend development, backend development, and database integration.
