import { useState } from 'react';
import AnimalModal from '../AnimalModal';
import { Button } from '@/components/ui/button';
import type { Animal } from '../AnimalCard';
import elephantImage from '@assets/generated_images/3D_cartoon_elephant_28b0801d.png';

const mockAnimal: Animal = {
  id: 'elephant-002',
  name: 'African Elephant',
  species: 'Loxodonta africana',
  habitat: 'African Savannas and Forests',
  rarity: 'legendary',
  imageUrl: elephantImage,
  facts: [
    'Elephants can weigh up to 13,000 pounds',
    'They have excellent memories and can remember other elephants for decades',
    'Elephants use their trunks like we use our hands',
    'Baby elephants are called calves and stay close to their mothers'
  ],
  collected: true
};

export default function AnimalModalExample() {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  return (
    <div className="p-4 space-y-4">
      <div className="flex gap-2">
        <Button onClick={() => setIsOpen(true)}>
          Open Animal Modal
        </Button>
        <Button 
          variant="outline" 
          onClick={() => setIsLoggedIn(!isLoggedIn)}
        >
          {isLoggedIn ? 'Logout' : 'Login'}
        </Button>
      </div>
      
      <AnimalModal
        animal={mockAnimal}
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        onAddToCollection={(id) => console.log('Added to collection:', id)}
        onAuthRequired={() => console.log('Auth required')}
        isLoggedIn={isLoggedIn}
      />
    </div>
  );
}