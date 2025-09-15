![WindowsAI hero image](docs/images/header.png)

# AI Dev Gallery — Personal Fork (Proof of Concept)

> **TL;DR**: This repository is **not** the official Microsoft project. It’s my personal fork used to showcase a **new Evaluation tab** concept I built as a proof-of-concept (POC).  
> If you’re looking for the real project, go here → **[microsoft/AI-Dev-Gallery](https://github.com/microsoft/AI-Dev-Gallery)**.

---

## Why this repo exists
I cloned the upstream repo and opened a couple of symbolic PRs in **this** fork to document and demonstrate the work I did (mostly front-end). Those PRs capture the design, UX, and implementation approach for an **Evaluation** experience inside AI Dev Gallery.

- 🤖 **Built with AI coding agents:** I used **Claude Code** and **GitHub Copilot** to accelerate scaffolding, refactors, and test stubs during this POC.
- 📦 This fork is for **show & tell** only (a POC / “vibe-coded” exploration).
- 🧭 For source of truth, docs, issues, and downloads, use the **official** repo linked above.

---

## What to look at (my work)
The two PRs below summarize the core of this POC and include descriptions/screenshots of the feature set.

1) **PR #5 – Evaluation Core Feature with Evaluation Wizard Dialog**  
   **Link:** https://github.com/BSPLAZA/ai-dev-gallery/pull/5  
   *Adds a complete Evaluation workflow and the new Evaluation tab foundation.*
   - 6-step **Evaluation Wizard** (workflow → type → model → dataset → metrics → review)
   - Three evaluation modes: **Test Model**, **Evaluate Responses**, **Import Results**
   - Evaluation management views (card + list), **search/filter**, bulk selection, and status tracking
   - Local data persistence and sample data generation
   - Keyboard navigation & screen-reader support (accessibility improvements)
## 📸 Screenshots
Evaluation Home
<img width="2538" height="1500" alt="Eval Home" src="https://github.com/user-attachments/assets/e5151c59-fcb4-41b4-9c59-6a2b20c409a0" />
Step 1 - Select Eval Type
<img width="2538" height="1500" alt="step1" src="https://github.com/user-attachments/assets/4ec099e5-153b-4b16-81e4-902521c97128" />

Step 2 - Choose Workflow
<img width="2538" height="1500" alt="step 2" src="https://github.com/user-attachments/assets/13dcc7d7-7dc2-4e8e-99b3-9dd16beff330" />

Step 3 - Configure Model with Image Description API Chosen
<img width="2756" height="1699" alt="step 3" src="https://github.com/user-attachments/assets/b21aeb62-3ea7-403a-972e-704723b7d264" />

Step 4 - Dataset Input 
<img width="2756" height="1699" alt="step 4" src="https://github.com/user-attachments/assets/fecbbe96-4cec-4cb2-a09f-34b50256b2fb" />

Step 5 - Evaluation Criteria
<img width="2756" height="1699" alt="step 5" src="https://github.com/user-attachments/assets/55bfd906-0e79-4888-bda8-61be3c47c822" />

Step 6 - Evaluation Confirmation
<img width="2756" height="1699" alt="step 6" src="https://github.com/user-attachments/assets/31d84ea7-6d19-4301-8084-f8ed8c3d39e4" />


2) **PR #6 – Evaluation Insights, Analytics, and Comparison Features**  
   **Link:** https://github.com/BSPLAZA/ai-dev-gallery/pull/6  
   *Builds on the core to add analytics and comparison.*
   - **Insights view** with score distributions and criteria breakdowns
   - **Compare** up to 4 evaluations side-by-side
   - Basic statistics (mean/median/std dev), trends, and item-level analysis
   - **Export** evaluation results (JSON/CSV)
   - Continued focus on performance and accessibility

## 📸 Screenshots
Evaluation Insights
<img width="3240" height="2064" alt="Eval Insights" src="https://github.com/user-attachments/assets/48e6ca58-eaf9-475b-9c71-7cabb7224b8c" />

Printable Report
<img width="3240" height="2064" alt="Printable Report" src="https://github.com/user-attachments/assets/796d0598-eeae-456d-8862-639c6658e8fb" />

Compare Evaluations
<img width="3240" height="2064" alt="Compare Evals" src="https://github.com/user-attachments/assets/229f3b3a-0136-4410-b43f-2adee9d2d096" />


> **Tip:** In each PR, click the **“Files changed”** tab to review the implementation details.

---

## Not the official project
- ⚠️ This fork isn’t maintained as a product. Please don’t open issues here for upstream bugs.
- 👉 Use **[microsoft/AI-Dev-Gallery](https://github.com/microsoft/AI-Dev-Gallery)** for releases, docs, and contributions.

---

## Credit
- Upstream project: **Microsoft — AI Dev Gallery**  
- AI coding agents used in this POC: **Claude Code** and **GitHub Copilot**  
- My contributions in this fork are for demonstration purposes only and do not imply Microsoft endorsement.

---

## Optional: local build instructions
If you want to build this fork locally, follow the official instructions from the upstream repo. The authoritative README and setup steps live there.

---

### Contact
If you have questions about this POC or want to discuss the Evaluation feature, feel free to reach out via GitHub.
