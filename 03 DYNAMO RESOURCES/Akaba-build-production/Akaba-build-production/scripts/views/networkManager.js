$(document).ready(function() {

   Groundhog.watchNetworkConnection(function(connected) {
      if (connected) {
         $("#disconnectionWarning").pulse("destroy")
         $("#disconnectionWarning").hide()
      } else {
         $("#disconnectionWarning").pulse({ 
            opacity: 0
         }, {
            duration : 900,
            interval : 250,
            pulses   : -1
         })
         $("#disconnectionWarning").show()
      }
   });

});

