---
name: mapbox-cartography
description: Expert guidance on map design principles, color theory, visual hierarchy, typography, and cartographic best practices for creating effective and beautiful maps with Mapbox. Use when designing map styles, choosing colors, or making cartographic decisions.
---

# Mapbox Cartography Skill

Expert guidance for creating effective and beautiful maps with Mapbox. Use this skill when designing map styles, choosing colors, typography, or making cartographic decisions.

---

## Core Cartographic Principles

### Visual Hierarchy

Establish clear visual hierarchy so users can quickly understand what matters most:

- **Primary features**: Roads, boundaries, and landmarks that support the map purpose should stand out
- **Secondary features**: Supporting context (parks, water bodies, buildings) should be visible but not compete for attention
- **Tertiary features**: Background elements (terrain, minor roads) should recede visually

**Techniques:**
- Use **size** (line width, point radius) to indicate importanceâ€”wider lines for major roads
- Use **color saturation**â€”saturated colors for primary, desaturated for background
- Use **opacity**â€”reduce opacity of less important layers
- Use **zoom-dependent visibility**â€”show detail only when zoomed in

### Color Theory for Maps

#### Color Harmony
- **Analogous**: Adjacent hues (e.g., blues and greens for water/parks)â€”creates calm, cohesive maps
- **Complementary**: Opposite hues (e.g., blue water vs. warm land)â€”creates strong contrast
- **Triadic**: Three evenly spaced huesâ€”use sparingly for accent elements
- **Monochromatic**: Single hue with varying saturation/lightnessâ€”elegant, minimal styles

#### Color Psychology
- **Blue**: Water, trust, calmâ€”standard for water bodies
- **Green**: Parks, nature, growthâ€”vegetation and open space
- **Red/Orange**: Alerts, heat, importanceâ€”use for highlights and POIs
- **Gray/Neutral**: Roads, buildingsâ€”neutral backgrounds that do not compete

#### Accessibility
- Ensure **sufficient contrast** between foreground and background (WCAG 4.5:1 for text, 3:1 for graphics)
- Avoid **red-green** as the only differentiatorâ€”add patterns, icons, or labels
- Test with **color blindness simulators** (protanopia, deuteranopia)
- Use **value (lightness)** differences in addition to hue for distinction

### Typography Best Practices

- **Font choice**: Use clear, legible fontsâ€”Mapbox fonts like Open Sans, Droid Sans, or system fonts
- **Label hierarchy**: Major labels (cities) larger and bolder than minor (neighborhoods)
- **Placement**: Avoid label overlap; use symbol-placement point or line for roads
- **Readability**: Light text on dark backgrounds; dark text on light backgrounds
- **Scale**: Labels should scale appropriately with zoomâ€”use text-size with zoom stops
- **Abbreviations**: Use sparingly; prefer full names at higher zoom levels

### Map Context Considerations

- **Purpose**: Design for the map primary useâ€”navigation needs different treatment than data exploration
- **Audience**: Technical vs. general audience affects complexity and labeling
- **Environment**: Consider light/dark mode, outdoor visibility, and screen size
- **Data density**: Balance information density with clarityâ€”do not overcrowd

---

## Mapbox-Specific Guidance

### Style Layer Best Practices

**Layer ordering** (bottom to top = back to front):
1. Background (fill)
2. Terrain/shading (if used)
3. Water (fill)
4. Land use (parks, forests)
5. Buildings (fill)
6. Roads (line)â€”order by importance: minor to major
7. Road labels
8. Points of interest (circles, icons)
9. POI labels
10. Custom data overlays
11. Annotations and highlights

**Key principles:**
- Place **data layers** above base map layers so they are visible
- Use **minzoom/maxzoom** to control when layers appear
- Group related layers and use consistent naming

### Zoom Level Strategy (0-22)

| Zoom | Typical Use | Detail Level |
|------|-------------|--------------|
| 0-2  | World/continent | Country labels, major boundaries |
| 3-5  | Regional | States, major cities |
| 6-8  | Metro area | Cities, highways |
| 9-11 | City | Neighborhoods, major streets |
| 12-14| Neighborhood | Streets, buildings |
| 15-17| Block | Building outlines, addresses |
| 18-22| Building/indoor | Fine detail, indoor features |

**Strategy:** Show less detail at low zoom; progressively add layers and increase label density as zoom increases. Use minzoom and maxzoom on layers and text-size/line-width with zoom stops.

### Color Palette Templates

#### Light Theme
```json
{
  "background": "#f8f9fa",
  "water": "#aad3df",
  "park": "#c8e6c9",
  "road": "#ffffff",
  "roadStroke": "#e0e0e0",
  "building": "#e8e8e8",
  "label": "#333333",
  "labelHalo": "#ffffff"
}
```

#### Dark Theme
```json
{
  "background": "#1a1a1a",
  "water": "#2c5282",
  "park": "#22543d",
  "road": "#2d3748",
  "roadStroke": "#4a5568",
  "building": "#2d3748",
  "label": "#e2e8f0",
  "labelHalo": "#1a1a1a"
}
```

#### High-Contrast Theme
```json
{
  "background": "#ffffff",
  "water": "#0066cc",
  "park": "#228b22",
  "road": "#000000",
  "roadStroke": "#333333",
  "building": "#d3d3d3",
  "label": "#000000",
  "labelHalo": "#ffffff"
}
```

#### Vintage Theme
```json
{
  "background": "#f5e6d3",
  "water": "#b8d4e3",
  "park": "#a8c9a8",
  "road": "#e8dcc8",
  "roadStroke": "#c4b59a",
  "building": "#e0d5c4",
  "label": "#5c4a3d",
  "labelHalo": "#f5e6d3"
}
```

---

## Common Mapping Scenarios

### Restaurant Finder
- **Base map**: Light, minimalâ€”avoid competing with POI markers
- **Markers**: Distinct icons or colors; consider clustering at low zoom
- **Labels**: Restaurant names on hover/click; avoid label clutter
- **Highlight**: Selected restaurant with larger marker and info popup

### Real Estate Map
- **Base map**: Neutral, professionalâ€”grays and soft colors
- **Property markers**: Size or color by price/availability
- **Boundaries**: Property outlines when zoomed in
- **Context**: Schools, transit, amenities as subtle overlays

### Data Visualization Overlay
- **Base map**: Muted or darkâ€”let data stand out
- **Choropleth**: Use sequential (single hue) or diverging (two hues) palettes
- **Points**: Size by value; color by category
- **Legend**: Always include for interpreted data

### Navigation/Routing
- **Route line**: High contrast (e.g., blue on light, cyan on dark)
- **Origin/destination**: Clear icons; consider pulsing for active navigation
- **Road hierarchy**: Emphasize route; dim non-route roads
- **Turn instructions**: Large, readable labels at decision points

---

## Performance Optimization

- **Simplify geometries**: Use lower detail at low zoom; increase at high zoom
- **Limit layers**: Each layer adds render costâ€”combine where possible
- **Use sprites efficiently**: Reuse icons; avoid excessive sprite sheets
- **Raster tiles**: For complex imagery, use raster tiles instead of vector
- **Lazy loading**: Load style layers only when needed (e.g., at certain zoom levels)
- **Reduce sources**: Minimize number of GeoJSON/tile sources

---

## Testing Your Design

- [ ] **Readability**: Can labels be read at intended zoom levels?
- [ ] **Contrast**: Do important elements stand out from the background?
- [ ] **Color blindness**: Test with simulators; ensure non-color cues exist
- [ ] **Zoom range**: Does the map work at min and max zoom?
- [ ] **Light/dark**: If supporting both, test in both modes
- [ ] **Mobile**: Check on small screensâ€”labels, touch targets
- [ ] **Performance**: Profile on low-end devices; check frame rate
- [ ] **Purpose**: Does the design support the map primary use case?

---

## Common Mistakes to Avoid

1. **Overcrowding**: Too many layers or labelsâ€”prioritize and simplify
2. **Poor contrast**: Low-contrast colors make maps hard to read
3. **Ignoring zoom**: Same styling at all zoom levelsâ€”use zoom-dependent properties
4. **Wrong layer order**: Data hidden behind base mapâ€”check layer stack
5. **Inconsistent colors**: Using different blues for waterâ€”establish a palette
6. **Accessibility neglect**: Relying only on colorâ€”add patterns, icons, labels
7. **Performance blindness**: Adding layers without testingâ€”profile and optimize
8. **Generic design**: Copying default styles without adapting to use caseâ€”customize for context

---

*Source: [Mapbox Agent Skills](https://github.com/mapbox/mapbox-agent-skills)*
