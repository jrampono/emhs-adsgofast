# Azure Data Services Go Fast Codebase

## Introduction

The Azure Data Services Go Fast Codebase is a combination of Microsoft components designed to shorten the "time to value" when deploying an Azure Data Platform. Key features include:

- Infrastucture as code (IAC) deployment of MVP Azure Data Platform 
- "Out of the box" Continuous Integration and Continuous Deployment framework  
- Enterpise grade security and monitoring with full support for Key Vault, VNETS, Private Enpoints and Managed Service Identities
- Codeless Ingestion from commonly used enterprise source systems into an enterprise data lake
- Users can interact with capabilities through a webpage and embedded dashboards.

This project is composed of Microsoft components and Open-Source Software (OSS) and is provided to customers and partners at no charge. 

At its core this project is intended to be an accelerator. As such, it is designed to accelerate the “time to value” in using the Microsoft components. As an accelerator, is it not for sale, nor is it a supported product.  

---

## Getting Started

### Prerequisites

Deployment of this project requires a variety of services across Azure. Please ensure that you have access to these services before continuing on to the deployment section of this guide.

>To get started you will need the following:
>
>- An active Azure Subscription & Empty Resource Group *
>- Contributor rights on the Azure Resource Group
>- Power BI Workspace

You can sign up for an Azure subscription [here](https://azure.microsoft.com/en-us/free/) 

Once you have your Prerequisite items, please move on to the Deployment Configuration step.

---

## Deployment Configuration

>You will also need some development tools to edit and run the deployment scripts provided. It is recommended you use the following:
>
>- A windows based computer (local or cloud)
>- [Visual Studio Code](https://visualstudio.microsoft.com/downloads/)
>- [Docker Desktop](https://www.docker.com/products/docker-desktop)
>- [Windows Store Ubuntu 18.04 LTS](https://www.microsoft.com/store/productId/9N9TNGVNDL3Q)

The deployment uses a concept of **Developing inside a Container** to containerize all the necessary pre-requisite component without requiring them to be installed on the local machine. Follow our [Configuring your System for Development Containers](https://code.visualstudio.com/docs/remote/containers) guide.

---

## Cost Estimator

Comming Soon.

---

## Deployment

Comming Soon.

---

## Navigating the Source Code

This  has the following structure:

Folder/File | Description
--- | ---
solution/ | Primary source code folder with sub-directories for each core technology
solution/Database | Contains source code for the meta-data database and sample databases
solution/Deployment | Contains CICD code 
solution/Diagrams | Contains a Structurizr diagramming project used for creation of architectural diagrams
solution/FunctionApp | Contains source code for the ADS Go Fast Orchestration Functions
solution/PowerBi | Contains source code for the Power BI files that can be used to provide reporting
solution/WebApplication | Contains source code for the ADS Go Fast web front end
---

## Contributing 

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Security
Microsoft takes the security of our software products and services seriously, which includes all source code repositories managed through our GitHub organizations, which include Microsoft, Azure, DotNet, AspNet, Xamarin, and our GitHub organizations. Please review this repository's [security section](../../security) for more details.

## Privacy
Microsoft values your privacy. See the [Microsoft Privacy Statement](https://privacy.microsoft.com/en-GB/data-privacy-notice) for more information

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
