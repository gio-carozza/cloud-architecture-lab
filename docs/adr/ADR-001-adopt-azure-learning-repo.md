# ADR-001: Adopt Azure as the Cloud Platform for the Learning Lab

## Status
Accepted

## Date
2026-03-15

## Context

To build hands-on experience in cloud architecture, a structured learning environment is required.

The environment must allow:

• experimentation with Azure services  
• documentation of architecture decisions  
• deployment of small cloud workloads  
• cost control during learning  

## Decision

Adopt Microsoft Azure as the primary cloud platform for this architecture learning lab.

Use a dedicated Azure subscription and a controlled resource group to host experimental workloads.

## Environment

Subscription: gio-architecture-lab

Resource Group: rg-ai-lab-dev-eastus

Budget Control: lab-monthly-limit

Region: East US

## Rationale

Azure is widely used for enterprise cloud platforms and provides strong support for:

• AI services  
• data platforms  
• enterprise architecture patterns  
• governance and security controls  

Learning Azure architecture principles will support future work as a Cloud AI Architect.

## Consequences

### Positive

• Real cloud environment for experimentation  
• Hands-on experience with Azure services  
• Cost monitoring through budget controls  

### Negative

• Requires discipline to manage costs  
• Requires learning Azure platform concepts