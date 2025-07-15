# ProceduralGenerations

**ProceduralGenerations** is a collection of procedural content generation algorithms implemented in **Godot Engine**, developed as part of a thesis project during an internship at **Cobryx S.r.l.** for the video game *Red Floor*.

## 📌 Overview

This repository contains multiple procedural generation modules designed to dynamically create various level layouts and environments. These generators can be used independently or combined to produce complex, varied, and non-repetitive game maps.

## 🧠 Implemented Generators

- **RoomGenerator** – Generates a collection of distinct rooms within a defined space.
- **GroupGenerator** – Places pre-defined groups of rooms or objects based on spatial constraints.
- **PrisonGenerator** – Builds prison-like layouts with corridors and cells in a grid-based format.
- **MazeGenerator** – Implements a classic maze generation algorithm.
- **CavesGenerator** – Uses cellular automata to create natural-looking cave systems.
- **PopulatedCavesGenerator** – Extends `CavesGenerator` by populating the cave system with entities or interactable objects.

## 🛠 Usage

Each generator is implemented as a separate script or scene within the Godot project.  
You can integrate them into your own Godot game by instantiating the relevant generator and invoking its `generate()` method.

## 📦 Requirements

- **Godot Engine 4.x** (recommended)
- Basic knowledge of GDScript and scene composition in Godot

## 📄 License

This project is released for academic and non-commercial use.
