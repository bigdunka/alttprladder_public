/****** Object:  Database [ALTTPR_Ladder_V2]    Script Date: 5/14/2025 8:57:54 PM ******/
CREATE DATABASE [ALTTPR_Ladder_V2]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ALTTPR_Ladder', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ALTTPR_Ladder_V2.mdf' , SIZE = 212416KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ALTTPR_Ladder_log', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ALTTPR_Ladder_V2_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ALTTPR_Ladder_V2].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET ARITHABORT OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET  DISABLE_BROKER 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET  MULTI_USER 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET DB_CHAINING OFF 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET QUERY_STORE = OFF
GO

ALTER DATABASE [ALTTPR_Ladder_V2] SET  READ_WRITE 
GO



USE [ALTTPR_Ladder_V2]
GO
/****** Object:  Table [dbo].[tb_api_requests]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_api_requests](
	[request_id] [int] IDENTITY(1,1) NOT NULL,
	[Request] [nvarchar](50) NOT NULL,
	[IPAddress] [nvarchar](50) NOT NULL,
	[RequestDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_tb_api_requests] PRIMARY KEY CLUSTERED 
(
	[request_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_branches]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_branches](
	[branch_id] [int] IDENTITY(1,1) NOT NULL,
	[EndPointURL] [nvarchar](255) NOT NULL,
	[SeedURL] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_tb_branches] PRIMARY KEY CLUSTERED 
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_browse_logs]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_browse_logs](
	[log_id] [int] IDENTITY(1,1) NOT NULL,
	[Request] [nvarchar](50) NOT NULL,
	[Parameter] [nvarchar](255) NULL,
	[IPAddress] [nvarchar](50) NOT NULL,
	[RequestDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_tb_browse_logs] PRIMARY KEY CLUSTERED 
(
	[log_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_decay]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_decay](
	[decay_id] [int] IDENTITY(1,1) NOT NULL,
	[racer_id] [int] NOT NULL,
	[flag_id] [int] NOT NULL,
	[DecayAccrued] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_tb_decay] PRIMARY KEY CLUSTERED 
(
	[decay_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_decay_BU]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_decay_BU](
	[decay_id] [int] IDENTITY(1,1) NOT NULL,
	[racer_id] [int] NOT NULL,
	[flag_id] [int] NOT NULL,
	[DecayAccrued] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_tb_decay_BU] PRIMARY KEY CLUSTERED 
(
	[decay_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_entrants]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_entrants](
	[entrant_id] [int] IDENTITY(1,1) NOT NULL,
	[race_id] [int] NOT NULL,
	[racer_id] [int] NOT NULL,
	[FinishTime] [int] NOT NULL,
	[DateTimeEntered] [datetime] NOT NULL,
	[DateTimeFinished] [datetime] NULL,
	[Comments] [nvarchar](max) NULL,
 CONSTRAINT [PK_tb_entrants] PRIMARY KEY CLUSTERED 
(
	[entrant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_flags]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_flags](
	[flag_id] [int] IDENTITY(1,1) NOT NULL,
	[FlagName] [nvarchar](255) NOT NULL,
	[FlagString] [nvarchar](max) NULL,
	[branch_id] [int] NOT NULL,
	[IsMystery] [bit] NOT NULL,
	[IsSpoiler] [bit] NOT NULL,
	[IsInvitational] [bit] NOT NULL,
	[IsGrabBag] [bit] NOT NULL,
	[HoursToComplete] [int] NOT NULL,
 CONSTRAINT [PK_tb_flags2] PRIMARY KEY CLUSTERED 
(
	[flag_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_gainpercentages]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_gainpercentages](
	[gain_id] [int] IDENTITY(1,1) NOT NULL,
	[RacerCount] [int] NOT NULL,
	[GainPerc] [decimal](8, 3) NOT NULL,
 CONSTRAINT [PK_tb_gainpercentages] PRIMARY KEY CLUSTERED 
(
	[gain_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_invitational_pairings]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_invitational_pairings](
	[pairing_id] [int] IDENTITY(1,1) NOT NULL,
	[invitational_id] [int] NOT NULL,
	[PairingID] [nvarchar](5) NOT NULL,
	[Week] [int] NOT NULL,
	[GroupName] [nvarchar](1) NOT NULL,
	[participant1_id] [int] NOT NULL,
	[participant2_id] [int] NOT NULL,
	[Participant1Time] [int] NULL,
	[Participant2Time] [int] NULL,
	[RaceDateTime] [datetime] NULL,
	[ThreadOpened] [bit] NOT NULL,
	[RaceRoom] [nvarchar](255) NULL,
	[RaceFinished] [bit] NOT NULL,
	[Participant1TrackingKey] [nvarchar](50) NULL,
	[Participant1RestreamKey] [nvarchar](50) NULL,
	[Participant2TrackingKey] [nvarchar](50) NULL,
	[Participant2RestreamKey] [nvarchar](50) NULL,
	[flag_id] [int] NULL,
	[EventID] [nvarchar](50) NULL,
	[NeedsUpdates] [bit] NOT NULL,
	[HasRestream] [bit] NOT NULL,
 CONSTRAINT [PK_tb_invitational_pairings] PRIMARY KEY CLUSTERED 
(
	[pairing_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_invitational_participants]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_invitational_participants](
	[participant_id] [int] IDENTITY(1,1) NOT NULL,
	[invitational_id] [int] NOT NULL,
	[racer_id] [int] NOT NULL,
	[RaceTimeGG] [nvarchar](255) NULL,
	[Seed] [int] NOT NULL,
	[SendTrackerCode] [bit] NOT NULL,
 CONSTRAINT [PK_tb_invitational_participant] PRIMARY KEY CLUSTERED 
(
	[participant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_invitationals]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_invitationals](
	[invitational_id] [int] IDENTITY(1,1) NOT NULL,
	[InvitationalName] [nvarchar](255) NOT NULL,
	[CurrentWeek] [int] NOT NULL,
 CONSTRAINT [PK_tb_invitationals] PRIMARY KEY CLUSTERED 
(
	[invitational_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_logs]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_logs](
	[log_id] [int] IDENTITY(1,1) NOT NULL,
	[LogMessage] [nvarchar](max) NULL,
	[DateTimeLogged] [datetime] NULL,
 CONSTRAINT [PK_tb_logs] PRIMARY KEY CLUSTERED 
(
	[log_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_processes]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_processes](
	[process_id] [int] IDENTITY(1,1) NOT NULL,
	[ProcessStart] [datetime] NOT NULL,
	[ProcessEnd] [datetime] NULL,
	[Messages] [text] NULL,
 CONSTRAINT [PK_tb_processes] PRIMARY KEY CLUSTERED 
(
	[process_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_racers]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_racers](
	[racer_id] [int] IDENTITY(1,1) NOT NULL,
	[old_racer_id] [int] NULL,
	[RacerGUID] [uniqueidentifier] NOT NULL,
	[RacerName] [nvarchar](255) NOT NULL,
	[RacerLogin] [nvarchar](255) NOT NULL,
	[DiscordID] [nvarchar](255) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[StreamURL] [nvarchar](255) NULL,
 CONSTRAINT [PK_tb_racers] PRIMARY KEY CLUSTERED 
(
	[racer_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_races]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_races](
	[race_id] [int] IDENTITY(1,1) NOT NULL,
	[RaceGUID] [uniqueidentifier] NOT NULL,
	[flag_id] [int] NOT NULL,
	[grabbag_id] [int] NULL,
	[Schedule] [int] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[RaceStartTime] [datetime] NULL,
	[SeedURL] [nvarchar](255) NULL,
	[MysteryJSON] [text] NULL,
	[HasStarted] [bit] NOT NULL,
	[HasCompleted] [bit] NOT NULL,
	[HasBeenProcessed] [bit] NULL,
	[ChampionshipRace] [bit] NOT NULL,
 CONSTRAINT [PK_tb_races] PRIMARY KEY CLUSTERED 
(
	[race_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_rankings]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_rankings](
	[ranking_id] [int] IDENTITY(1,1) NOT NULL,
	[racer_id] [int] NOT NULL,
	[flag_id] [int] NOT NULL,
	[race_id] [int] NOT NULL,
	[Ranking] [int] NOT NULL,
	[Change] [int] NOT NULL,
	[Result] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_tb_rankings] PRIMARY KEY CLUSTERED 
(
	[ranking_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_rankings_BU]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_rankings_BU](
	[ranking_id] [int] IDENTITY(1,1) NOT NULL,
	[racer_id] [int] NOT NULL,
	[flag_id] [int] NOT NULL,
	[race_id] [int] NOT NULL,
	[Ranking] [int] NOT NULL,
	[Change] [int] NOT NULL,
	[Result] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_tb_rankings_BU] PRIMARY KEY CLUSTERED 
(
	[ranking_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_rankings_cache]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_rankings_cache](
	[cache_id] [int] IDENTITY(1,1) NOT NULL,
	[flag_id] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
	[racer_id] [int] NOT NULL,
	[RacerName] [nvarchar](255) NOT NULL,
	[Ranking] [int] NOT NULL,
	[Rank] [int] NOT NULL,
	[GainLoss] [int] NOT NULL,
	[TotalRaces] [int] NOT NULL,
	[Firsts] [int] NOT NULL,
	[Seconds] [int] NOT NULL,
	[Thirds] [int] NOT NULL,
	[Forfeits] [int] NOT NULL,
 CONSTRAINT [PK_tb_rankings_cache] PRIMARY KEY CLUSTERED 
(
	[cache_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_schedule]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_schedule](
	[schedule_id] [int] IDENTITY(1,1) NOT NULL,
	[flag_id] [int] NOT NULL,
	[ScheduleGroup] [int] NOT NULL,
	[SeedOrder] [int] NOT NULL,
 CONSTRAINT [PK_tb_schedule] PRIMARY KEY CLUSTERED 
(
	[schedule_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_seed_tracking]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_seed_tracking](
	[tracking_id] [int] IDENTITY(1,1) NOT NULL,
	[seed_id] [int] NOT NULL,
	[IPAddress] [nvarchar](50) NOT NULL,
	[DownloadDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_tb_seed_tracking] PRIMARY KEY CLUSTERED 
(
	[tracking_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_seeds]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_seeds](
	[seed_id] [int] IDENTITY(1,1) NOT NULL,
	[race_id] [int] NOT NULL,
	[SeedHash] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_tb_seeds] PRIMARY KEY CLUSTERED 
(
	[seed_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tb_spoilers]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_spoilers](
	[spoiler_id] [int] IDENTITY(1,1) NOT NULL,
	[race_id] [int] NOT NULL,
	[SpoilerHash] [nvarchar](32) NULL,
	[FileLocation] [nvarchar](max) NULL,
 CONSTRAINT [PK_tb_spoilers] PRIMARY KEY CLUSTERED 
(
	[spoiler_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[tb_invitational_pairings] ADD  CONSTRAINT [DF_tb_invitational_pairings_ThreadOpened_1]  DEFAULT ((0)) FOR [ThreadOpened]
GO
ALTER TABLE [dbo].[tb_invitational_pairings] ADD  CONSTRAINT [DF_tb_invitational_pairings_RaceFinished_1]  DEFAULT ((0)) FOR [RaceFinished]
GO
ALTER TABLE [dbo].[tb_invitational_pairings] ADD  CONSTRAINT [DF_tb_invitational_pairings_NeedsUpdates]  DEFAULT ((0)) FOR [NeedsUpdates]
GO
ALTER TABLE [dbo].[tb_invitational_pairings] ADD  CONSTRAINT [DF_tb_invitational_pairings_HasRestream]  DEFAULT ((0)) FOR [HasRestream]
GO
ALTER TABLE [dbo].[tb_invitational_participants] ADD  CONSTRAINT [DF_tb_invitational_participants_SendTrackerCode_1]  DEFAULT ((0)) FOR [SendTrackerCode]
GO
ALTER TABLE [dbo].[tb_racers] ADD  CONSTRAINT [DF_tb_racers_RacerGUID]  DEFAULT (newid()) FOR [RacerGUID]
GO
ALTER TABLE [dbo].[tb_races] ADD  CONSTRAINT [DF_tb_races_RaceGUID]  DEFAULT (newid()) FOR [RaceGUID]
GO
ALTER TABLE [dbo].[tb_races] ADD  CONSTRAINT [DF_tb_races_ChampionshipRace]  DEFAULT ((0)) FOR [ChampionshipRace]
GO
/****** Object:  StoredProcedure [dbo].[sp_CacheRankings]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[sp_CacheRankings]
	@flag_id int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @tmpx int
	
	CREATE TABLE #tmp
	(
		racer_id int,
		RacerName nvarchar(255),
		Ranking int,
		[Rank] int,
		GainLoss int,
		TotalRaces int,
		Firsts int,
		Seconds int,
		Thirds int,
		Forfeits int
	)

	--DECLARE db_cursor CURSOR FOR SELECT 0 UNION SELECT flag_id FROM tb_flags

	--OPEN db_cursor
	--FETCH NEXT FROM db_cursor INTO @flag_id

	--WHILE @@FETCH_STATUS = 0  
	--BEGIN
		TRUNCATE TABLE #tmp

		INSERT INTO #tmp (racer_id)
		SELECT DISTINCT tb_rankings.racer_id FROM tb_rankings INNER JOIN tb_racers ON tb_racers.racer_id = tb_rankings.racer_id WHERE flag_id = @flag_id AND race_id <> 0 AND tb_racers.IsActive = 1

		UPDATE #tmp SET RacerName = (SELECT RacerName FROM tb_racers WHERE tb_racers.racer_id = #tmp.racer_id)

		UPDATE #tmp SET Ranking = (SELECT TOP 1 Ranking FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND flag_id = @flag_id ORDER BY LastUpdated DESC), 
		GainLoss = (SELECT TOP 1 [Change] FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND flag_id = @flag_id ORDER BY LastUpdated DESC),
		TotalRaces = (SELECT COUNT(*) FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND flag_id = @flag_id AND race_id > 0),
		Firsts = (SELECT COUNT(*) FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND flag_id = @flag_id AND Result = 1),
		Seconds = (SELECT COUNT(*) FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND flag_id = @flag_id AND Result = 2),
		Thirds = (SELECT COUNT(*) FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND flag_id = @flag_id AND Result = 3),
		Forfeits = (SELECT COUNT(*) FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND flag_id = @flag_id AND Result = 999)

		WHILE (SELECT COUNT(*) FROM #tmp WHERE [Rank] IS NULL) > 0
		BEGIN
			SET @tmpx = (SELECT TOP 1 Ranking FROM #tmp WHERE [Rank] IS NULL ORDER BY Ranking DESC)

			UPDATE #tmp SET [Rank] = (SELECT COUNT(*) + 1 FROM #tmp WHERE [Rank] IS NOT NULL) WHERE Ranking = @tmpx
		END

		DELETE FROM tb_rankings_cache WHERE flag_id = @flag_id
		INSERT INTO tb_rankings_cache ([flag_id],[LastUpdated],[racer_id],[RacerName],[Ranking],[Rank],[GainLoss],[TotalRaces],[Firsts],[Seconds],[Thirds],[Forfeits])
		SELECT @flag_id, GETDATE(), racer_id, RacerName, Ranking, [Rank], GainLoss, TotalRaces, Firsts, Seconds, Thirds, Forfeits FROM #tmp ORDER BY [Rank], RacerName

	--	FETCH NEXT FROM db_cursor INTO @flag_id 
	--END 

	--CLOSE db_cursor  
	--DEALLOCATE db_cursor

	DROP TABLE #tmp

END
GO
/****** Object:  StoredProcedure [dbo].[sp_CalculateRaces]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_CalculateRaces]

AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #tmp_flag
	(
		flag_id int
	)

	DECLARE @race_id int, @flag_id int

	DECLARE db_calc CURSOR FOR 
	SELECT race_id 
	FROM tb_races WHERE HasBeenProcessed = 0 AND HasCompleted = 1

	OPEN db_calc
	FETCH NEXT FROM db_calc INTO @race_id

	WHILE @@FETCH_STATUS = 0  
	BEGIN
		SET @flag_id = (SELECT flag_id FROM tb_races WHERE race_id = @race_id)
		
		INSERT INTO #tmp_flag (flag_id)
		SELECT @flag_id WHERE NOT EXISTS (SELECT flag_id FROM #tmp_flag WHERE flag_id = @flag_id)

		--EXEC sp_CalculateRankings @race_id, 0
		EXEC sp_CalculateRankings @race_id

		UPDATE tb_races SET HasBeenProcessed = 1 WHERE race_id = @race_id

		FETCH NEXT FROM db_calc INTO @race_id 
	END 

	CLOSE db_calc  
	DEALLOCATE db_calc

	DECLARE db_proc CURSOR FOR 
	SELECT flag_id 
	FROM #tmp_flag

	OPEN db_proc
	FETCH NEXT FROM db_proc INTO @flag_id

	WHILE @@FETCH_STATUS = 0  
	BEGIN
		--EXEC sp_ProcessDecay @flag_id

		EXEC sp_CacheRankings @flag_id

		FETCH NEXT FROM db_proc INTO @flag_id 
	END 

	CLOSE db_proc  
	DEALLOCATE db_proc

	DROP TABLE #tmp_flag
END


GO
/****** Object:  StoredProcedure [dbo].[sp_CalculateRankings]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_CalculateRankings]
	@race_id int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @flag_id int, @total_racers int, @total_rating int, @total_wager int, @wager_remaining int, @tmpx int, @tmpy int, @tmpz int, @perc_gain decimal(12, 3), @tmp_id int, @last_finish int, @tmp_rating int, @last_rating int
	
	PRINT @race_id

	CREATE TABLE #tmp
	(
		tmp_id int IDENTITY(1, 1) primary key,
		racer_id int,
		FinishTime int,
		StartingRating int,
		Wager int,
		Result int,
		ResultPerc int,
		Gain int,
		Remainder int,
		Reward int,
		NewRating int,
		GainLoss int,
		PlacementPerc decimal(8,2),
		PlacementBonus int
	)

	SET @flag_id = (SELECT flag_id FROM tb_races WHERE race_id = @race_id)

	INSERT INTO #tmp (racer_id, FinishTime, Remainder)
	SELECT racer_id, FinishTime, 0 FROM tb_entrants WHERE race_id = @race_id
	ORDER BY FinishTime

	UPDATE #tmp SET StartingRating = (SELECT TOP 1 Ranking FROM tb_rankings WHERE #tmp.racer_id = tb_rankings.racer_id AND tb_rankings.flag_id = @flag_id ORDER BY LastUpdated DESC)

	DECLARE db_cursor CURSOR FOR 
	SELECT racer_id 
	FROM #tmp WHERE StartingRating IS NULL

	OPEN db_cursor
	FETCH NEXT FROM db_cursor INTO @tmp_id  

	WHILE @@FETCH_STATUS = 0  
	BEGIN
		INSERT INTO tb_rankings (racer_id, flag_id, race_id, Ranking, Change, Result, LastUpdated)
		SELECT @tmp_id, @flag_id, 0, 1000, 0, 0, GETDATE()
		
		UPDATE #tmp SET StartingRating = 1000 WHERE racer_id = @tmp_id

		FETCH NEXT FROM db_cursor INTO @tmp_id 
	END 

	CLOSE db_cursor  
	DEALLOCATE db_cursor

	SET @total_racers = (SELECT COUNT(*) FROM #tmp)
	SET @total_rating = (SELECT SUM(StartingRating) FROM #tmp)

	UPDATE #tmp SET Wager = StartingRating * .025

	SET @total_wager = (SELECT SUM(Wager) FROM #tmp)
	SET @wager_remaining = @total_wager

	SET @perc_gain = (SELECT GainPerc FROM tb_gainpercentages WHERE RacerCount = @total_racers) -- ROUND((0.41 - (@total_racers * 0.005)), 3)

	IF @perc_gain IS NULL
	BEGIN
		SET @perc_gain = (SELECT TOP 1 GainPerc FROM tb_gainpercentages ORDER BY RacerCount DESC)
	END

	SET @tmp_rating = 0
	SET @last_rating = 0
	SET @last_finish = 0

	--PRINT @total_wager
	--PRINT @perc_gain

	DECLARE db_cursor CURSOR FOR 
	SELECT tmp_id 
	FROM #tmp

	OPEN db_cursor
	FETCH NEXT FROM db_cursor INTO @tmp_id  

	WHILE @@FETCH_STATUS = 0  
	BEGIN
		SET @tmp_rating = @tmp_rating + 1
		
		IF (SELECT FinishTime FROM #tmp WHERE tmp_id = @tmp_id) <> @last_finish
		BEGIN
			UPDATE #tmp SET Result = CASE WHEN FinishTime = 99999 THEN @total_racers ELSE @tmp_rating END WHERE tmp_id = @tmp_id
			SET @last_finish = (SELECT FinishTime FROM #tmp WHERE tmp_id = @tmp_id)
			SET @last_rating = (SELECT Result FROM #tmp WHERE tmp_id = @tmp_id)
		END
		ELSE
		BEGIN
			UPDATE #tmp SET Result = @last_rating WHERE tmp_id = @tmp_id
		END

		UPDATE #tmp SET ResultPerc = ((100 / @total_racers) * (Result - 1)) + 1 WHERE tmp_id = @tmp_id

		UPDATE #tmp SET Gain = ROUND(@wager_remaining * @perc_gain, 0) WHERE tmp_id = @tmp_id

		IF (SELECT Gain FROM #tmp WHERE tmp_id = @tmp_id) < (SELECT Wager + (Wager * 0.05) FROM #tmp WHERE tmp_id = @tmp_id) AND (SELECT ResultPerc FROM #tmp WHERE tmp_id = @tmp_id) < 20
		BEGIN
			UPDATE #tmp SET Gain = Wager + (Wager * 0.05) WHERE tmp_id = @tmp_id
		END

		SET @wager_remaining = @total_wager - (SELECT SUM(Gain) FROM #tmp)

		FETCH NEXT FROM db_cursor INTO @tmp_id 
	END 

	CLOSE db_cursor  
	DEALLOCATE db_cursor

	IF (SELECT COUNT(Result) FROM #tmp WHERE FinishTime <> 99999) <> (SELECT COUNT(DISTINCT Result) FROM #tmp WHERE FinishTime <> 99999)
	BEGIN
		--PRINT @race_id

		DECLARE db_cursor CURSOR FOR 
		SELECT Result 
		FROM #tmp

		OPEN db_cursor
		FETCH NEXT FROM db_cursor INTO @tmp_id  

		WHILE @@FETCH_STATUS = 0  
		BEGIN
			UPDATE #tmp SET Gain = ROUND((SELECT SUM(Gain) FROM #tmp WHERE Result = @tmp_id) / (SELECT COUNT(*) FROM #tmp WHERE Result = @tmp_id), 0) WHERE Result = @tmp_id

			FETCH NEXT FROM db_cursor INTO @tmp_id 
		END 

		CLOSE db_cursor  
		DEALLOCATE db_cursor

	END

	SET @tmpy = 1

	WHILE @wager_remaining > 0
	BEGIN
		SET @tmpx = 1

		WHILE @tmpx <= @tmpy AND @wager_remaining > 0
		BEGIN
			SET @tmpz = (SELECT COUNT(*) FROM #tmp WHERE Result = @tmpx)
			IF @tmpz > 1
			BEGIN
				IF @tmpz <= @wager_remaining
				BEGIN
					UPDATE #tmp SET Remainder = Remainder + 1 WHERE Result = @tmpx
					SET @wager_remaining = @wager_remaining - @tmpz
				END
				ELSE
				BEGIN
					DECLARE @tmp_id2 int

					DECLARE db_cursor_rem CURSOR FOR SELECT racer_id FROM #tmp WHERE Result = @tmpx ORDER BY Wager DESC

					OPEN db_cursor_rem
					FETCH NEXT FROM db_cursor_rem INTO @tmp_id2  

					WHILE @@FETCH_STATUS = 0 AND @wager_remaining > 0
					BEGIN
						UPDATE #tmp SET Remainder = Remainder + 1 WHERE racer_id = @tmp_id2  
						SET @wager_remaining = @wager_remaining - 1

						FETCH NEXT FROM db_cursor_rem INTO @tmp_id2
					END 

					CLOSE db_cursor_rem  
					DEALLOCATE db_cursor_rem

					SET @wager_remaining = 0
				END
			END
			ELSE
			BEGIN
				UPDATE #tmp SET Remainder = Remainder + 1 WHERE Result = @tmpx
				SET @wager_remaining = @wager_remaining - 1
			END

			SET @tmpx = @tmpx + 1
			
		END

		SET @tmpy = @tmpy + 1
	END

	UPDATE #tmp SET PlacementPerc = ROUND(((101.0 - ResultPerc ) / 100.0) / 3, 2, 2)
	UPDATE #tmp SET PlacementPerc = 0 WHERE FinishTime = 99999
	UPDATE #tmp SET PlacementBonus = @total_racers * PlacementPerc

	UPDATE #tmp SET Reward = Gain + Remainder + PlacementBonus
	UPDATE #tmp SET NewRating = StartingRating - Wager + Reward
	UPDATE #tmp SET GainLoss = NewRating - StartingRating 

	INSERT INTO tb_rankings ([racer_id],[flag_id],[race_id],[Ranking],[Change],[Result],[LastUpdated])
	SELECT racer_id, @flag_id, @race_id, NewRating, GainLoss, CASE WHEN FinishTime = 99999 THEN 999 ELSE Result END, GETDATE() FROM #tmp

	--SELECT * FROM #tmp

	DROP TABLE #tmp
END


GO
/****** Object:  StoredProcedure [dbo].[sp_ProcessDecay]    Script Date: 5/14/2025 8:58:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[sp_ProcessDecay]
	@flag_id int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE  @tmp_racer int, @tmp_decay int

	CREATE TABLE #tmp_flags
	(
		flag_id int
	)

	CREATE TABLE #tmp_racers
	(
		racer_id int,
		LastRaced datetime,
		LastDecay datetime,
		DecayCap int,
		DecayAccrued int,
		LastRanking int,
		NewRecord bit
	)

	TRUNCATE TABLE #tmp_racers

	INSERT INTO #tmp_racers (racer_id)
	SELECT DISTINCT racer_id FROM tb_rankings WHERE flag_id = @flag_id AND race_id <> 0

	UPDATE #tmp_racers SET LastRaced = (SELECT TOP 1 RaceStartTime FROM tb_races INNER JOIN tb_rankings ON tb_rankings.race_id = tb_races.race_id WHERE racer_id = #tmp_racers.racer_id AND HasBeenProcessed = 1 AND DATEDIFF(DAY, RaceStartTime, GETDATE()) > 90 ORDER BY RaceStartTime DESC)

	UPDATE #tmp_racers SET LastDecay = ISNULL((SELECT LastUpdated FROM tb_decay WHERE racer_id = #tmp_racers.racer_id AND flag_id = @flag_id), '1-1-2000')

	DELETE FROM #tmp_racers WHERE DATEDIFF(DAY, LastDecay, GETDATE()) < 7

	UPDATE #tmp_racers SET DecayCap = (SELECT COUNT(*) FROM tb_rankings INNER JOIN tb_races ON tb_races.race_id = tb_rankings.race_id WHERE racer_id = #tmp_racers.racer_id AND HasBeenProcessed = 1 AND DATEDIFF(DAY, RaceStartTime, GETDATE()) < 365) * 5

	UPDATE #tmp_racers SET DecayCap = 30 WHERE DecayCap > 30

	DELETE FROM #tmp_racers WHERE DecayCap = 0

	UPDATE #tmp_racers SET DecayAccrued = (SELECT DecayAccrued FROM tb_decay WHERE racer_id = #tmp_racers.racer_id AND flag_id = @flag_id)
		
	DELETE FROM #tmp_racers WHERE DecayAccrued >= DecayCap

	UPDATE #tmp_racers SET NewRecord = 1 WHERE DecayAccrued IS NULL
	UPDATE #tmp_racers SET NewRecord = 0 WHERE DecayAccrued IS NOT NULL

	UPDATE #tmp_racers SET DecayAccrued = 0 WHERE DecayAccrued IS NULL

	UPDATE #tmp_racers SET LastRanking = (SELECT TOP 1 Ranking FROM tb_rankings WHERE racer_id = #tmp_racers.racer_id AND flag_id = @flag_id AND race_id > 0 ORDER BY LastUpdated DESC)

	DECLARE db_cursor_racers CURSOR FOR SELECT racer_id FROM #tmp_racers

	OPEN db_cursor_racers
	FETCH NEXT FROM db_cursor_racers INTO @tmp_racer

	WHILE @@FETCH_STATUS = 0  
	BEGIN
		IF (SELECT NewRecord FROM #tmp_racers WHERE racer_id = @tmp_racer) = 1
		BEGIN
			INSERT INTO tb_decay (racer_id, flag_id, DecayAccrued, LastUpdated)
			SELECT @tmp_racer, @flag_id, 5, GETDATE()
		END
		ELSE
		BEGIN
			UPDATE tb_decay SET DecayAccrued = DecayAccrued + 5, LastUpdated = GETDATE() WHERE racer_id = @tmp_racer AND flag_id = @flag_id
		END

		INSERT INTO tb_rankings (racer_id, flag_id, race_id, Ranking, Change, Result, LastUpdated)
		SELECT @tmp_racer, @flag_id, -1, (SELECT TOP 1 Ranking FROM tb_rankings WHERE racer_id = @tmp_racer AND flag_id = @flag_id AND race_id <> 0 ORDER BY LastUpdated DESC) + ROUND(((SELECT LastRanking FROM #tmp_racers WHERE racer_id = @tmp_racer) * -0.05), 0), Change = ROUND(((SELECT LastRanking FROM #tmp_racers WHERE racer_id = @tmp_racer) * -0.05), 0), -1, GETDATE()

		FETCH NEXT FROM db_cursor_racers INTO @tmp_racer
	END 

	CLOSE db_cursor_racers  
	DEALLOCATE db_cursor_racers

	DROP TABLE #tmp_racers


END



GO
