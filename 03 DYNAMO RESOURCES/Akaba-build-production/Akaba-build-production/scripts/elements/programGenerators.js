define(function() {
   window.defaultPrograms = {};

   var sqft_to_sqm = 0.092903;

   window.defaultPrograms.wellnessCenter = [
      {
         name: "Exercise-Lobby",
         area: 3000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Pool/Aquatic Center",
         area: 10000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Track",
         area: 5000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Gymnasia",
         area: 6000 * sqft_to_sqm,
         quantity:6
      },
      {
         name: "Weight/Equipment Room",
         area: 2000 * sqft_to_sqm,
         quantity:2
      },
      {
         name: "Locker Room",
         area: 2000 * sqft_to_sqm,
         quantity:2
      },
      {
         name: "Wellness",
         area: 8000 * sqft_to_sqm,
         quantity:8
      },
      {
         name: "Exercise-Education/Outreach",
         area: 4000 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Exercise-Staff",
         area: 2000 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Exercise-Administration",
         area: 1000 * sqft_to_sqm,
         quantity:2
      },
      {
         name: "Exercise-Storage",
         area: 1000 * sqft_to_sqm,
         quantity:5
      },
      {
         name: "Nutrition-Lobby",
         area: 3000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Local Foods Market",
         area: 10000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Outlets",
         area: 8000 * sqft_to_sqm,
         quantity:8
      },
      {
         name: "Nutrition-Education/Outreach",
         area: 4000 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Nutrition-Staff",
         area: 2000 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Nutrition-Administration",
         area: 1000 * sqft_to_sqm,
         quantity:2
      },
      {
         name: "Nutrition-Storage",
         area: 1000 * sqft_to_sqm,
         quantity:5
      },
      {
         name: "Spiritual-Lobby",
         area: 3000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Chapel",
         area: 8000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Spiritual-Education/Outreach",
         area: 4000 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Spiritual-Staff",
         area: 2000 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Spiritual-Administration",
         area: 1000 * sqft_to_sqm,
         quantity:2
      },
      {
         name: "Spiritual-Storage",
         area: 1000 * sqft_to_sqm,
         quantity:5
      },
      {
         name: "Open office",
         area: 360000 * sqft_to_sqm,
         quantity:18
      }
   ];

   window.defaultPrograms.highSchool = [
      {
         name: "Reception",
         area: 250 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Director's Office",
         area: 300 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Asst.Dir. Office",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Finance",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Marketing",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Administrative Assistant",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Conference",
         area: 300 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Kitchen/Break Room",
         area: 200 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Work Room",
         area: 200 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Server closet",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Executive Toilet Room",
         area: 100 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Permanent Faculty Offices",
         area: 3600 * sqft_to_sqm,
         quantity:24
      },
      {
         name: "Library",
         area: 4000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Library Office",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Digital Room",
         area: 500 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Print Room",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Tech Staff",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Photography Studio",
         area: 150 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Storage",
         area: 200 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Cafeteria",
         area: 6000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Kitchen",
         area: 15000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Classrooms",
         area: 48000 * sqft_to_sqm,
         quantity:24
      },
      {
         name: "Tech Classrooms",
         area: 12000 * sqft_to_sqm,
         quantity:8
      },
      {
         name: "Gymnasium",
         area: 12000 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Studio Spaces",
         area: 2400 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Lockers/Toilets",
         area: 3200 * sqft_to_sqm,
         quantity:2
      },
      {
         name: "General Storage",
         area: 4000 * sqft_to_sqm,
         quantity:4
      },
      {
         name: "Secure Storage",
         area: 250 * sqft_to_sqm,
         quantity:1
      },
      {
         name: "Open Office",
         area: 500000 * sqft_to_sqm,
         quantity:25
      }
   ];

   window.defaultPrograms.simplifiedWellnessCenter = [
      {
         name: "Lobbies",
         area: 300,
         quantity: 3
      },
      {
         name: "Pool/Aquatic Center",
         area: 1000,
         quantity: 1
      },
      {
         name: "Track",
         area: 500,
         quantity: 1
      },
      {
         name: "Gymnasia",
         area: 600,
         quantity: 6
      },
      {
         name: "Weight/Equipment/Locker Room",
         area: 800,
         quantity: 4
      },
      {
         name: "Education/Outreach",
         area: 1200,
         quantity: 12
      },
      {
         name: "Foods Market/Outlets",
         area: 1200,
         quantity: 16
      },
      {
         name: "Outlets",
         area: 800,
         quantity: 8
      },
      {
         name: "Staff/Admin",
         area: 800,
         quantity: 20
      },
      {
         name: "Chapel",
         area: 800,
         quantity: 1
      },
      {
         name: "Open Office",
         area: 40000,
         quantity: 20
      }
   ];

   window.defaultPrograms.simplifiedHighSchool = [
      {
         name: "Permanent Faculty",
         area: 480,
         quantity:24
      },
      {
         name: "Cafeteria",
         area: 1400,
         quantity: 1
      },
      {
         name: "Kitchen",
         area: 600,
         quantity: 1
      },
      {
         name: "Classrooms",
         area: 6400,
         quantity: 24
      },
      {
         name: "Tech Classrooms",
         area: 5600,
         quantity: 8
      },
      {
         name: "Gymnasium",
         area: 2000,
         quantity: 1
      },
      {
         name: "Studio Spaces",
         area: 1600,
         quantity: 4
      },
      {
         name: "Lockers/Toilets",
         area: 300,
         quantity: 2
      },
      {
         name: "General Storage",
         area: 400,
         quantity: 4
      },
      {
         name: "Open Office",
         area: 50000,
         quantity: 50
      }
   ];

   window.defaultPrograms.simplifiedBotanicalGardenMuseum = [
      {
         name: "Exhibition Galleries",
         area: 3600,
         quantity:10
      },
      {
         name: "Project Space/Atrium",
         area: 500,
         quantity: 1
      },
      {
         name: "Ground Floor Lobby",
         area: 500,
         quantity: 1
      },
      {
         name: "Museum and Design Store",
         area: 300,
         quantity: 1
      },
      {
         name: "Ticketing/Info",
         area: 200,
         quantity: 1
      },
      {
         name: "Cafe/Bar/Rest",
         area: 600,
         quantity: 1
      },
      {
         name: "Administrative Offices",
         area: 450,
         quantity: 9
      },
      {
         name: "Administrative Support",
         area: 200,
         quantity: 8
      },
      {
         name: "Conference Rooms",
         area: 100,
         quantity: 4
      },
      {
         name: "Open Office",
         area: 60000,
         quantity: 80
      }
   ];

   window.defaultPrograms.botanicalGardenMuseum = [
      {
         name: "Exhibition Galleries",
         area: 39000 * sqft_to_sqm
      },
      {
         name: "Project Space/Atrium",
         area: 3000 * sqft_to_sqm
      },
      {
         name: "Visitor Screening/Bag Check",
         area: 1000 * sqft_to_sqm
      },
      {
         name: "Coat Check/Lockers",
         area: 600 * sqft_to_sqm
      },
      {
         name: "Ticketing and Information Desk",
         area: 200 * sqft_to_sqm
      },
      {
         name: "Storage",
         area: 100 * sqft_to_sqm
      },
      {
         name: "Museum and Design Store",
         area: 2500 * sqft_to_sqm
      },
      {
         name: "Stock Room and Offices",
         area: 500 * sqft_to_sqm
      },
      {
         name: "Cafe/Bar",
         area: 2000 * sqft_to_sqm
      },
      {
         name: "Kitchen",
         area: 3700 * sqft_to_sqm
      },
      {
         name: "Administrative Offices",
         area: 1300 * sqft_to_sqm
      },
      {
         name: "Curatorial, Exhibition Design, Publications, Archivist offices",
         area: 1100 * sqft_to_sqm
      },
      {
         name: "Education offices",
         area: 300 * sqft_to_sqm
      },
      {
         name: "Marketing and Development Offices",
         area: 1000 * sqft_to_sqm
      },
      {
         name: "Conference Rooms",
         area: 750 * sqft_to_sqm
      },
      {
         name: "Shared Work Room/Copy Room/File Storage",
         area: 550 * sqft_to_sqm
      },

      {
         name: "Security Office/Control Room",
         area: 200 * sqft_to_sqm
      },
      {
         name: "Custodial Office",
         area: 200 * sqft_to_sqm
      },
      {
         name: "IT Server, Workroom, and Staff Offices",
         area: 350 * sqft_to_sqm
      },
      {
         name: "Supply, Equipment, and Seasonal Furniture Storage",
         area: 400 * sqft_to_sqm
      },
      {
         name: "Landscape and Grounds Maintenance Equipment",
         area: 500 * sqft_to_sqm
      },
      {
         name: "Staff Lunch Room/Lounge",
         area: 650 * sqft_to_sqm
      },
      {
         name: "Locker Rooms",
         area: 250 * sqft_to_sqm
      },
      {
         name: "Lobbies",
         area: 5000 * sqft_to_sqm
      }
   ];

   window.defaultPrograms.hospital = [
      {
         name: "public",
         area: 125
      },
      {
         name: "emergency",
         area: 150,
         adjacencies: [
            {
               space: "public",
               distanceTo: 30
            },
            {
               space: "pharmacy",
               distanceTo: 30
            },
            {
               space: "laundry",
               distanceTo: 100
            },
            {
               space: "records",
               distanceTo: 30
            }
         ]
      },
      {
         name: "laboratory",
         area: 30
      },
      {
         name: "imaging",
         area: 100,
         adjacencies: [
            {
               space: "laboratory",
               distanceTo: 10
            },
            {
               space: "public",
               distanceTo: 10
            }
         ]
      },
      {
         name: "records",
         area: 75
      },
      {
         name: "inpatient care",
         area: 550,
         adjacencies: [
            {
               space: "emergency",
               distanceTo: 20
            },
            {
               space: "imaging",
               distanceTo: 40
            },
            {
               space: "pharmacy",
               distanceTo: 30
            },
            {
               space: "laundry",
               distanceTo: 100
            },
            {
               space: "public",
               distanceTo: 30
            }
         ]
      },
      {
         name: "administrative",
         area: 175,
         adjacencies: [
            {
               space: "records",
               distanceTo: 10
            },
            {
               space: "public",
               distanceTo: 30
            }
         ]
      },
      {
         name: "materials management",
         area: 90,
         adjacencies: [
            {
               space: "public",
               distanceTo: 30
            }
         ]
      },
      {
         name: "pharmacy",
         area: 50,
         adjacencies: [
            {
               space: "public",
               distanceTo: 30
            }
         ]
      },
      {
         name: "maintenance",
         area: 90,
         adjacencies: [
            {
               space: "public",
               distanceTo: 100
            }
         ]
      },
      {
         name: "laundry",
         area: 110
      }
   ];

   window.defaultPrograms.custom = [
      {
         name: "conference",
         area: 460,
         adjacencies: [
            {
               space: "lobby",
               distanceTo: 300
            }
         ]
      },
      {
         name: "lobby",
         area: 100,
         adjacencies: [
            {
               space: "conference",
               distanceTo: 30
            }
         ]
      },
      { 
         name: "office",
         area: 1300,
         adjacencies: [
            {
               space: "conference",
               distanceTo: 30
            },
            {
               space: "restroom",
               distanceTo: 40
            },
            {
               space: "lobby",
               distanceTo: 300
            }
         ]
      },
      {
         name: "restroom",
         area: 100,
         adjacencies: [
            {
               space: "restroom",
               distanceTo: 300
            }
         ]
      }
   ];

   window.defaultPrograms.userDefined = [];

   window.createProgramFromPreset = function(preset) {
      var programData = new ProgramData("program");
      window.preAddElementForTransaction(programData.uniqueId);
      programData.uses = {};
      programData.programReqs = {};
      programData.proximityReqs = {};

      var namesToId = {};

      // Store space requirements
      _.each(preset, function(space) {
         var newId = getNewId();
         var newColor = getNextColorIndex(programData);

         programData.programReqs[newId] = {
           requiredArea: space.area
         };

         if (space.quantity!=null) 
            programData.programReqs[newId].quantity = space.quantity;
      
         programData.uses[newId] = {
           name: space.name,
           colorIndex: newColor
         };

         namesToId[space.name] = newId;
      });

      // Store adjacency requirements
      _.each(preset, function(space) {
         var adjacencies = space.adjacencies;
         if (adjacencies == null)
            return;

         var proximities = {};
         proximities.distanceTo = {};
         _.each(adjacencies, function(adjacency) {
            proximities.distanceTo[namesToId[adjacency.space]] = adjacency.distanceTo;
         });
         programData.proximityReqs[namesToId[space.name]] = proximities;
      });


      Groundhog.addElement(programData, {global: true});
   };
});