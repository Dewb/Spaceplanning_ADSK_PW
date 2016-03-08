cmake_minimum_required (VERSION 2.6)
project (ShellStufferService)

add_executable (ShellStufferService
	Stuffer/Application/stdafx.cpp
	Stuffer/Application/Stuffer.cpp

	Stuffer/Casablanca/GeneratorListener.cpp
	Stuffer/Casablanca/JobRequest.cpp
	Stuffer/Casablanca/JSONUtil.cpp
	Stuffer/Casablanca/LayoutJob.cpp
	Stuffer/Casablanca/SpaceLayout.cpp
	Stuffer/Casablanca/StufferJob.cpp
	Stuffer/Casablanca/StufferListener.cpp

	Stuffer/Generator/Analyzer/UsageAnalyzer.cpp

	Stuffer/Generator/Genotype/Genome.cpp
	Stuffer/Generator/Genotype/Ribosome.cpp

	Stuffer/Generator/Optimizer/HillClimbingOptimizer.cpp

	Stuffer/Generator/Phenotype/Adjacency.cpp
	Stuffer/Generator/Phenotype/AdjacencyGraph.cpp
	Stuffer/Generator/Phenotype/Floor.cpp
	Stuffer/Generator/Phenotype/Phenotype.cpp
	Stuffer/Generator/Phenotype/Section.cpp
	Stuffer/Generator/Phenotype/Usage.cpp

	Stuffer/Generator/Plan/Area.cpp
	Stuffer/Generator/Plan/Args.cpp
	Stuffer/Generator/Plan/BuildData.cpp
	Stuffer/Generator/Plan/Cell.cpp
	Stuffer/Generator/Plan/CellHelperConnectedUp.cpp
	Stuffer/Generator/Plan/CellHelperNotClaimed.cpp
	Stuffer/Generator/Plan/CellScanner.cpp
	Stuffer/Generator/Plan/EdgeVisitor.cpp
	Stuffer/Generator/Plan/JobData.cpp
	Stuffer/Generator/Plan/Plan.cpp
	Stuffer/Generator/Plan/Shaft.cpp

	Stuffer/Generator/Shell/Axis.cpp
	Stuffer/Generator/Shell/Grid.cpp
	Stuffer/Generator/Shell/Level.cpp
	Stuffer/Generator/Shell/RectShell.cpp
	Stuffer/Generator/Shell/STLShell.cpp
	Stuffer/Generator/Shell/STLxShell.cpp
	Stuffer/Generator/Shell/Shell.cpp

	Stuffer/Generator/Strategy/BuildData/AreaDivisionStrategy.cpp
	Stuffer/Generator/Strategy/BuildData/EdgeSpacesStrategy.cpp
	Stuffer/Generator/Strategy/BuildData/LevelConnectionStrategy.cpp
	Stuffer/Generator/Strategy/BuildData/OutdoorConnectionStrategy.cpp
	Stuffer/Generator/Strategy/BuildData/RingHallwayStrategy.cpp

	Stuffer/Generator/Strategy/Phenotype/CreateSpaceStrategy.cpp
	Stuffer/Generator/Strategy/Phenotype/CreateStairsStrategy.cpp
	Stuffer/Generator/Strategy/Phenotype/DominoAddSpaceStrategy.cpp
	Stuffer/Generator/Strategy/Phenotype/DominoBacktrackStrategy.cpp
	Stuffer/Generator/Strategy/Phenotype/ExistingDesignStrategy.cpp
	Stuffer/Generator/Strategy/Phenotype/SetUsageStrategy.cpp
	Stuffer/Generator/Strategy/Phenotype/ShellStrategy.cpp
	Stuffer/Generator/Strategy/Phenotype/SpaceDivisionStrategy.cpp

	Stuffer/Generator/Strategy/System/ChoiceStrategy.cpp
	Stuffer/Generator/Strategy/System/EdgeEffectStrategy.cpp
	Stuffer/Generator/Strategy/System/ListStrategy.cpp
	Stuffer/Generator/Strategy/System/LoopStrategy.cpp
	Stuffer/Generator/Strategy/System/OptimizingStrategy.cpp
	Stuffer/Generator/Strategy/System/SideStrategy.cpp

	Stuffer/Generator/Utility/Bag/Bag.cpp
	Stuffer/Generator/Utility/Bag/BagCache.cpp

	Stuffer/Generator/Utility/Grid/GridBasis.cpp
	Stuffer/Generator/Utility/Grid/GridPoint.cpp
	Stuffer/Generator/Utility/Grid/GridRef.cpp
	Stuffer/Generator/Utility/Grid/GridScanner.cpp
	Stuffer/Generator/Utility/Grid/GridValue.cpp

	Stuffer/Generator/Vision/BuildingCoreVision.cpp
	Stuffer/Generator/Vision/DominoVision.cpp
	Stuffer/Generator/Vision/EdgeEffectTestVision.cpp
	Stuffer/Generator/Vision/ShellAndCoreVision.cpp
	Stuffer/Generator/Vision/StackedVision.cpp
	Stuffer/Generator/Vision/StairPlacementVision.cpp
	Stuffer/Generator/Vision/TrapelloVision.cpp
	Stuffer/Generator/Vision/Vision.cpp
	Stuffer/Generator/Vision/XYVision.cpp
)

include_directories (
	Stuffer/Application/Include
	Stuffer/Casablanca/Include

	Stuffer/Generator/Analyzer/Include
	Stuffer/Generator/Genotype/Include
	Stuffer/Generator/Optimizer/Include
	Stuffer/Generator/Phenotype/Include
	Stuffer/Generator/Plan/Include
	Stuffer/Generator/Shell/Include
	Stuffer/Generator/Strategy/BuildData/Include
	Stuffer/Generator/Strategy/Phenotype/Include
	Stuffer/Generator/Strategy/System/Include
	Stuffer/Generator/Utility/Bag/Include
	Stuffer/Generator/Utility/Grid/Include
	Stuffer/Generator/Utility/Include
	Stuffer/Generator/Display/Include/API
	Stuffer/Generator/Vision/Include

	Stuffer/Lib/Casablanca/Include
)

# Boost
find_package(Boost COMPONENTS system thread chrono program_options REQUIRED)
target_link_libraries (ShellStufferService
    ${Boost_LIBRARIES}
)

find_library(
	CPPREST
	NAMES cpprest libcpprest.2.4.dylib
	PATHS
		/usr/lib
		/usr/local/lib
		/usr/lib/i386-linux-gnu
		lib/casablanca/lib/osx
)
target_link_libraries (ShellStufferService
	${CPPREST}
    ${Boost_LIBRARIES}
)

set(CMAKE_CXX_FLAGS "-std=c++11")
set(CMAKE_BUILD_TYPE "RelWithDebInfo")

install(TARGETS ShellStufferService DESTINATION bin/)
