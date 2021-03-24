/****** Object:  Table [dbo].[AUFinancialLicenses]    Script Date: 8/03/2021 10:56:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AUFinancialLicenses](
	[REGISTER_NAME] [nvarchar](max) NULL,
	[AFS_LIC_NUM] [nvarchar](max) NULL,
	[AFS_LIC_NAME] [nvarchar](max) NULL,
	[AFS_LIC_ABN_ACN] [nvarchar](max) NULL,
	[AFS_LIC_START_DT] [nvarchar](max) NULL,
	[AFS_LIC_PRE_FSR] [nvarchar](max) NULL,
	[AFS_LIC_ADD_LOCAL] [nvarchar](max) NULL,
	[AFS_LIC_ADD_STATE] [nvarchar](max) NULL,
	[AFS_LIC_ADD_PCODE] [nvarchar](max) NULL,
	[AFS_LIC_ADD_COUNTRY] [nvarchar](max) NULL,
	[AFS_LIC_LAT] [nvarchar](max) NULL,
	[AFS_LIC_LNG] [nvarchar](max) NULL,
	[AFS_LIC_CONDITION] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BrisbHospRegDeaths]    Script Date: 8/03/2021 10:56:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BrisbHospRegDeaths](
	[Last name] [varchar](128) NULL,
	[Given names] [varchar](128) NULL,
	[Age] [varchar](128) NULL,
	[Ordinal no] [varchar](128) NULL,
	[Date of death] [varchar](128) NULL,
	[Prev sys] [varchar](128) NULL,
	[Item ID] [varchar](128) NULL,
	[Notes] [varchar](2000) NULL,
	[Index name] [varchar](128) NULL,
	[Description] [varchar](128) NULL,
	[Source] [varchar](128) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NYTaxiYellowTripData]    Script Date: 8/03/2021 10:56:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NYTaxiYellowTripData](
	[VendorID] [int] NOT NULL,
	[tpep_pickup_datetime] [datetime] NOT NULL,
	[tpep_dropoff_datetime] [datetime] NOT NULL,
	[passenger_count] [smallint] NOT NULL,
	[trip_distance] [decimal](8, 2) NOT NULL,
	[RatecodeID] [smallint] NOT NULL,
	[store_and_fwd_flag] [char](1) NOT NULL,
	[PULocationID] [int] NOT NULL,
	[DOLocationID] [int] NOT NULL,
	[payment_type] [smallint] NULL,
	[fare_amount] [decimal](10, 2) NULL,
	[extra] [decimal](10, 2) NULL,
	[mta_tax] [decimal](10, 2) NULL,
	[tip_amount] [decimal](10, 2) NULL,
	[tolls_amount] [decimal](10, 2) NULL,
	[improvement_surcharge] [decimal](10, 2) NULL,
	[total_amount] [decimal](10, 2) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[stg_AUFinancialLicenses]    Script Date: 8/03/2021 10:56:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[stg_AUFinancialLicenses](
	[REGISTER_NAME] [nvarchar](max) NULL,
	[AFS_LIC_NUM] [nvarchar](max) NULL,
	[AFS_LIC_NAME] [nvarchar](max) NULL,
	[AFS_LIC_ABN_ACN] [nvarchar](max) NULL,
	[AFS_LIC_START_DT] [nvarchar](max) NULL,
	[AFS_LIC_PRE_FSR] [nvarchar](max) NULL,
	[AFS_LIC_ADD_LOCAL] [nvarchar](max) NULL,
	[AFS_LIC_ADD_STATE] [nvarchar](max) NULL,
	[AFS_LIC_ADD_PCODE] [nvarchar](max) NULL,
	[AFS_LIC_ADD_COUNTRY] [nvarchar](max) NULL,
	[AFS_LIC_LAT] [nvarchar](max) NULL,
	[AFS_LIC_LNG] [nvarchar](max) NULL,
	[AFS_LIC_CONDITION] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[stg_BrisbHospRegDeaths]    Script Date: 8/03/2021 10:56:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[stg_BrisbHospRegDeaths](
	[Last name] [varchar](128) NULL,
	[Given names] [varchar](128) NULL,
	[Age] [varchar](128) NULL,
	[Ordinal no] [varchar](128) NULL,
	[Date of death] [varchar](128) NULL,
	[Prev sys] [varchar](128) NULL,
	[Item ID] [varchar](128) NULL,
	[Notes] [varchar](2000) NULL,
	[Index name] [varchar](128) NULL,
	[Description] [varchar](128) NULL,
	[Source] [varchar](128) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[stg_NYTaxiYellowTripData]    Script Date: 8/03/2021 10:56:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[stg_NYTaxiYellowTripData](
	[VendorID] [int] NOT NULL,
	[tpep_pickup_datetime] [datetime] NOT NULL,
	[tpep_dropoff_datetime] [datetime] NOT NULL,
	[passenger_count] [smallint] NULL,
	[trip_distance] [decimal](8, 2) NULL,
	[RatecodeID] [smallint] NOT NULL,
	[store_and_fwd_flag] [char](1) NULL,
	[PULocationID] [int] NOT NULL,
	[DOLocationID] [int] NOT NULL,
	[payment_type] [smallint] NULL,
	[fare_amount] [decimal](10, 2) NULL,
	[extra] [decimal](10, 2) NULL,
	[mta_tax] [decimal](10, 2) NULL,
	[tip_amount] [decimal](10, 2) NULL,
	[tolls_amount] [decimal](10, 2) NULL,
	[improvement_surcharge] [decimal](10, 2) NULL,
	[total_amount] [decimal](10, 2) NULL
) ON [PRIMARY]
GO
