---
name: pcmiler
description: PCMiler REST API provides methods to retrieve a series of geocoding, routing, and mileage data
---
When the user asks about routes, mileage, distances, geocoding, or map-related
queries, use the PCMiler REST API to fulfill the request.

## API Configuration
- **Base URL**: `https://pcmiler.alk.com/apis/rest/v1.0/Service.svc`
- **Route Reports Endpoint**: `https://pcmiler.alk.com/apis/rest/v1.0/Service.svc/route/routeReports`
- **Auth Token**: Passed as query parameter `authToken` (stored in miniagent.json under `skills.entries.pcmiler.authToken`)
- **Docs**: https://developer.trimblemaps.com/restful-apis/routing/route-reports/get-route-reports/

## Authentication
All API requests must include the query parameter:
```
?authToken={TOKEN_FROM_CONFIG}
```

## Route Reports (GET)

### Required Parameters
- **stops**: Coordinates as comma-separated longitude,latitude pairs, separated by semicolons.
  Format: `lon1,lat1;lon2,lat2`
  Example: `-74.005974,40.712776;-87.629799,41.878113` (New York to Chicago)
- **reports**: Comma-separated list of report types to retrieve.

### Available Report Types
| Report | Description |
|--------|-------------|
| Mileage | Distance, time, and cost for each stop |
| Detailed | Comprehensive report for each leg |
| CalcMiles | Calculate route distance |
| Directions | Turn-by-turn directions |
| Geotunnel | Series of lat/lon points along route |
| LeastCost | Comparison of multiple possible routes |
| Road | Distance breakdown by road category |
| State | Mileage info per state/country traveled |
| RoutePath | Geographic coordinates making up the route |
| WeatherAlerts | Weather alerts along the route |

### Common Route Options
| Parameter | Description | Default |
|-----------|-------------|---------|
| vehType | 0=Truck, 1=LightTruck, 2=Auto, 3=Midsize Truck | 0 (Truck) |
| routeType | 0=Practical, 1=Shortest, 2=Fastest | 0 (Practical) |
| distUnits | 0=Miles, 1=Km | 0 (Miles) |
| useTraffic | Use traffic data (true/false) | false |
| tollRoads | 1=Always Avoid, 2=Avoid if Possible, 3=Use | null |
| hwyOnly | Highways only routing | false (GET) |

### Example API Calls

**Mileage between New York and Chicago:**
```
GET /route/routeReports?stops=-74.005974,40.712776;-87.629799,41.878113&reports=CalcMiles&authToken={TOKEN}
```

**Directions with vehicle type Auto:**
```
GET /route/routeReports?stops=-74.005974,40.712776;-87.629799,41.878113&reports=Directions&vehType=2&authToken={TOKEN}
```

**State-by-state mileage report:**
```
GET /route/routeReports?stops=-74.005974,40.712776;-87.629799,41.878113&reports=State&authToken={TOKEN}
```

## Usage Steps
1. Read the auth token from miniagent.json config using the file_read tool.
2. Construct the REST API URL with stops as `lon,lat;lon,lat` and desired reports.
3. Use the shell tool to execute the HTTP request (e.g. `curl`).
4. Parse the JSON response and present results clearly to the user.

## Example User Queries
- "What is the driving distance from New York to Chicago?"
- "Get turn-by-turn directions from Los Angeles to San Francisco"
- "Show state-by-state mileage from Dallas to Miami"
- "How long does it take to drive from Seattle to Portland?"
- "Calculate the shortest route from Princeton, NJ to Philadelphia, PA"
