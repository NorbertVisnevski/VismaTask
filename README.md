# Console application to manage Visma’s internal meetings using .NET6

### Available commands:

- add-meeting [name] [responsible person] [description] [category] [type] [start date] [end date]  -  adds meeting
    
- remove-meeting [name] [responsible person]  -  removes meeting if person is authorized to do it

- list-meetings  -  lists all meeting by specified parameters
    
  Optional filters:
  
        -d  [description] 
        -c  [category] 
        -t  [type] 
        -u  [responsible person] 
        -s  [start date] 
        -e  [end date] 
        -aa [attending above count] 
        -ab [attending below count] 
        -ai [attending includes user]
    
- add-to-meeting [name] [person] [attendint start date]  -  adds attendee to meeting
    
- remove-from-meeting [name] [person]  -  removes attendee from meeting
        
Required parameters [parameter] should be omitted in interactive mode, except for filter options

To enter interactive mode just run the application with 0 initial parameters

Application meeting data is saved in data.json file
