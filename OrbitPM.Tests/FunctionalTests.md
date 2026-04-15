# OrbitPM Functional Test Documentation

This document outlines the high-level functional test cases ensuring the core requirements of the OrbitPM system are met, specifically focusing on the Blind-Match workflow and the Role-Based Access Control (RBAC) registration.

## 1. Student Project Submission (Blind Entry)
**Test Objective:** Ensure students can submit proposals without supervisor details being visible initially.
- **Pre-condition:** User logged in as Student.
- **Steps:**
  1. Navigate to "Submit Proposal".
  2. Fill in Title, Abstract, Technical Stack, and select a Research Area.
  3. Submit the form.
- **Expected Outcome:** 
  - Proposal is saved in the database with status `Pending`.
  - Record is created in `ProposalOwnership` table.
  - No `MatchRecord` exists initially.

## 2. Supervisor Anonymized Browsing (Blind-Match)
**Test Objective:** Ensure supervisors can browse proposals without knowing student identities.
- **Pre-condition:** User logged in as Supervisor.
- **Steps:**
  1. Navigate to "Available Proposals".
- **Expected Outcome:** 
  - List of `Pending` proposals is displayed.
  - Student names/IDs are NOT visible on the UI or in the DOM.
  - Proposals can be filtered by `ResearchArea`.

## 3. Match Fulfillment (Unlocking Identity)
**Test Objective:** Verify the state transition from 'Pending' to 'Matched' and the "identity unlocking" logic.
- **Pre-condition:** User logged in as Supervisor, at least one `Pending` proposal exists.
- **Steps:**
  1. Click "Express Interest" on a specific proposal.
- **Expected Outcome:**
  - Proposal status changes to `Matched`.
  - A `MatchRecord` is created linking the Supervisor and Proposal.
  - System displays a confirmation message: "Identity link has been successfully unlocked".
  - The proposal no longer appears in the "Available" list for other supervisors.

## 4. RBAC Registration Enforcement
**Test Objective:** Ensure only appropriate roles can register or be created.
- **Pre-condition:** Unauthenticated user at registration page.
- **Steps:**
  1. Attempt to register with role 'Admin' or 'Module Leader' (if available).
- **Expected Outcome:**
  - Self-registration is restricted to 'Student' (or as per project configuration).
  - Cross-registration (Module Leader creating another Module Leader) is restricted (Test Case based on line 778120f commit).

## 5. Persistence Recovery
**Test Objective:** Ensure data survives server restarts.
- **Steps:**
  1. Create a proposal and match it.
  2. Restart the application.
  3. View matched projects as Supervisor.
- **Expected Outcome:** Data remains consistent and status remains `Matched`.
