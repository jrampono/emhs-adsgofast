use staging
go

--SELECT  
--     N'DROP TABLE [SalesLT].' + QUOTENAME(TABLE_NAME) 
--FROM INFORMATION_SCHEMA.TABLES
--WHERE TABLE_SCHEMA = 'SalesLT' 


DROP TABLE [SalesLT].[SalesOrderDetail]
DROP TABLE [SalesLT].[SalesOrderHeader]
DROP TABLE [SalesLT].[ProductModel]
DROP TABLE [SalesLT].[ProductCategory]
DROP TABLE [SalesLT].[CustomerAddress]
DROP TABLE [SalesLT].[stg_Customer]
DROP TABLE [SalesLT].[stg_ProductModel]
DROP TABLE [SalesLT].[stg_Product]
DROP TABLE [SalesLT].[stg_ProductDescription]
DROP TABLE [SalesLT].[Product]
DROP TABLE [SalesLT].[stg_ProductModelProductDescription]
DROP TABLE [SalesLT].[ProductDescription]
DROP TABLE [SalesLT].[stg_ProductCategory]
DROP TABLE [SalesLT].[stg_Address]
DROP TABLE [SalesLT].[stg_CustomerAddress]
DROP TABLE [SalesLT].[Customer]
DROP TABLE [SalesLT].[stg_SalesOrderDetail]
DROP TABLE [SalesLT].[Address]
DROP TABLE [SalesLT].[stg_SalesOrderHeader]
DROP TABLE [SalesLT].[ProductModelProductDescription]
DROP TABLE [dbo].[ErrorLog]
DROP TABLE [dbo].[BuildVersion]
DROP TABLE [dbo].[stg_ErrorLog]
DROP TABLE [dbo].[stg_BuildVersion]