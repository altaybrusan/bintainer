# Bintainer

## About Bintainer

With over a decade of experience in developing embedded systems, we recognized the critical need for efficient inventory management. **Bintainer** was developed at **X-Lab** in collaboration with **Boğaziçi University**, under the supervision of **Prof. Dr. Cengizhan Öztürk**.

This innovative solution is actively used in X-Lab, managing hundreds of component types across electronics, electrical, and mechanical parts. Designed to enhance efficiency and productivity, Bintainer is tailored to meet the demanding needs of embedded systems development.

### Key Features

* **Track Inventory**
  Order components before they reach low levels, ensuring smooth project workflow.

* **Speed Up**
  Quickly locate components in bin hives to accelerate soldering tasks.

* **Stay Updated**
  Track components from global suppliers and receive warnings about deprecated tools, ensuring your projects stay up-to-date.

---

## Inventory Organization

In laboratories, components are usually arranged according to their **physical specifications** or **material applications**. For instance, in an electronics lab:

* **Resistors** are stored in nearby boxes.
* These clusters are known as **inventory sections**.

Each section contains:

* A **width**: max number of bins horizontally.
* A **height**: max number of boxes vertically.
* There may be **missing bins** in each section.

![Inventory Example](https://github.com/user-attachments/assets/645ba4ea-3455-4adb-b932-5e9be0343aef)

Example: Width = 6, Height = 18 — 4 missing boxes in the section.

---

## Bin Organization

**Bins** are containers that hold components.

* Each bin can have **sub-spaces**.
* Each sub-space can store a **different part**.

![Bin Example](https://github.com/user-attachments/assets/71cf5b30-d83c-4f3f-b3b2-e426649d4bd0)

---

## Group Organization

**Groups** are logical collections of related components:

* A group may span **multiple sections**.
* Useful for organizing components based on **application**.
* Example: A `USB Group` includes voltage regulators, capacitors, and resistors.

![Group Example](https://github.com/user-attachments/assets/b0a6da98-0f1a-4ed9-bcd2-bb13bf05fe59)

Benefits of grouping:

* Easier to **locate** parts
* Simplifies **availability checks** for sub-circuits

---

## How to Compile

Bintainer is developed on **.NET 7** and requires **MS SQL Server 17 (2020 or higher)**.

### Step-by-Step Setup

1. **Clone the Repository**

2. **Deploy the Database**

#### Step 1: Select the `Bintainer.Db` Project

![Step 1](https://github.com/user-attachments/assets/c54ac3fb-bc68-4327-b569-a4f29d8824a7)

#### Step 2: Right-click and Select `Publish`

![Step 2](https://github.com/user-attachments/assets/79503ac6-eb7f-4364-a28c-e7dc96fd4ad5)

#### Step 3: Publish Screen Will Appear

![Step 3](https://github.com/user-attachments/assets/fd0cb52c-6c4f-4b1b-922d-43ed796bda43)

#### Step 4: Choose the Target SQL Server Instance

![Step 4](https://github.com/user-attachments/assets/34b52a35-772e-47ad-ab27-b15a5fe08a4c)

#### Step 5: Name the Database (`EtrekDb`) and Press **Publish**

![Step 5](https://github.com/user-attachments/assets/af41b84d-daf1-4b4b-ab81-ddb1fb6f06c8)

#### Step 6: Confirm the Database Deployment

![Step 6](https://github.com/user-attachments/assets/f0e61aef-7c0a-42a0-9ae9-cc21cddd68d4)

#### Step 7: Copy the Connection String

* Go to **Database Properties**
* Copy the **Connection String**
* Paste into your `appsettings.json` or `secrets` file

![Step 7](https://github.com/user-attachments/assets/b9619c3b-c064-4ed4-b237-60817c47bea7)

---

## Requirements

* .NET 7 SDK
* SQL Server 2017 or later
* Visual Studio 2022 or newer

---

If you need help converting this to `HTML`, `LaTeX`, or `PDF`, or want a more advanced developer or user guide structure (e.g., sidebar navigation, API references), let me know!


