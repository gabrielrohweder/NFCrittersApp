# Design Guidelines: NFC Animal Collection Mobile App

## Design Approach: Reference-Based (Mobile Gaming/Collection Apps)
Drawing inspiration from successful collection apps like Pokémon GO, Duolingo, and educational apps like WWF Free Rivers. The design emphasizes playful discovery, achievement satisfaction, and mobile-first interactions.

## Core Design Principles
- **Mobile-first immersion**: Every element optimized for single-hand phone usage
- **Playful discovery**: Each scan feels like unwrapping a surprise
- **Achievement satisfaction**: Visual rewards for collection progress
- **Educational engagement**: Facts presented in digestible, engaging formats

## Color Palette
**Primary Brand Colors:**
- Deep Forest Green: 150 85% 25% (main brand, trust, nature)
- Vibrant Lime: 75 80% 55% (discovery moments, success states)

**Supporting Colors:**
- Warm Cream: 45 25% 92% (light backgrounds, cards)
- Rich Charcoal: 220 15% 15% (dark text, night mode)
- Soft Coral: 15 75% 70% (call-to-action accents)

**Gradients:**
- Hero sections: Deep forest to lime (150 85% 25% to 75 80% 55%)
- Success animations: Lime to coral (75 80% 55% to 15 75% 70%)
- Background overlays: Subtle cream variations (45 25% 92% to 45 15% 88%)

## Typography
**Primary:** Inter (Google Fonts) - clean, mobile-readable
**Secondary:** Fredoka One (Google Fonts) - playful headings for animal names
- Headers: 24px+ Fredoka One, bold
- Body: 16px Inter, regular
- Captions: 14px Inter, medium
- Buttons: 16px Inter, semibold

## Layout System
**Spacing:** Tailwind units 2, 4, 6, 8, 12
- Tight spacing (p-2, m-2): Icon spacing, small gaps
- Standard spacing (p-4, m-4): Card padding, form elements  
- Generous spacing (p-6, p-8): Section padding, major separators
- Large spacing (p-12): Page margins, hero sections

## Component Library

**Core UI Elements:**
- **Animal Cards:** Rounded corners (rounded-xl), soft shadows, cream backgrounds with gradient overlays on hover
- **Scan Button:** Large, circular, lime gradient with pulsing animation indicating NFC readiness
- **Progress Indicators:** Playful progress bars with animal silhouettes showing collection completion

**Navigation:**
- **Bottom Tab Bar:** Fixed navigation with large touch targets (min 44px)
- **Back Navigation:** Swipe gestures supported, clear arrow indicators

**Forms:**
- **Authentication Modal:** Slide-up overlay with blurred background
- **Input Fields:** Rounded, generous padding, clear focus states with lime accent
- **Buttons:** Primary (lime gradient), Secondary (outline with blur background when over images)

**Data Displays:**
- **Collection Grid:** 2-column grid on mobile, 3D tilt effects on animal cards
- **Animal Detail View:** Full-screen modal with animated animal image, expandable fact sections
- **Achievement Badges:** Circular badges with gradient backgrounds for milestones

**Overlays:**
- **Scan Success:** Full-screen celebration animation with confetti and animal reveal
- **Loading States:** Skeleton screens with gentle pulsing, animal-themed loading animations

## Images
**Animal Illustrations:** 
- Style: 3D-rendered, slightly cartoonish but realistic animals
- Format: High-quality PNGs with transparent backgrounds
- Animation: Subtle floating/breathing animations, scale transforms on interaction
- Placement: Hero sections of animal detail pages, collection grid thumbnails

**Background Elements:**
- Subtle nature-inspired patterns (leaf silhouettes, geometric forest shapes)
- Gradient overlays on hero sections
- No large hero images - focus remains on individual animal discoveries

**Icons:**
- Use Heroicons for UI elements (scan, collection, profile, settings)
- Custom animal category icons as placeholders (<!-- CUSTOM ICON: paw print, wings, fins -->)

## Animations
**Discovery Moments:**
- Scan success: Scale-up reveal with particle effects
- New animal: Gentle bounce-in animation
- Collection addition: Satisfying snap-to-grid animation

**Micro-interactions:**
- Button press: Subtle scale down (0.95x)
- Card hover: Gentle lift with shadow increase
- Progress updates: Smooth bar fills with easing

**Page Transitions:**
- Slide transitions between main sections
- Modal slide-up for authentication and details
- Gentle fade for content updates

This design creates an engaging, mobile-optimized experience that makes each animal discovery feel special while maintaining educational value and collection satisfaction.