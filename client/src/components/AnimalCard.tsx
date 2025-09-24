import { motion } from "framer-motion";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

export interface Animal {
  id: string;
  name: string;
  species: string;
  habitat: string;
  rarity: 'common' | 'rare' | 'legendary';
  imageUrl: string;
  facts: string[];
  collected?: boolean;
}

interface AnimalCardProps {
  animal: Animal;
  onClick?: () => void;
  isNew?: boolean;
}

export default function AnimalCard({ animal, onClick, isNew = false }: AnimalCardProps) {
  const rarityColors = {
    common: 'bg-secondary',
    rare: 'bg-accent',
    legendary: 'bg-primary'
  };

  return (
    <motion.div
      initial={isNew ? { scale: 0, rotate: -10 } : false}
      animate={isNew ? { scale: 1, rotate: 0 } : {}}
      transition={{ type: "spring", stiffness: 300, damping: 20 }}
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
    >
      <Card 
        className={`hover-elevate cursor-pointer overflow-hidden ${isNew ? 'ring-2 ring-primary' : ''}`}
        onClick={onClick}
        data-testid={`card-animal-${animal.id}`}
      >
        <CardContent className="p-0">
          <div className="relative aspect-square">
            <img 
              src={animal.imageUrl} 
              alt={animal.name}
              className="w-full h-full object-cover bg-gradient-to-br from-background to-muted"
              data-testid={`img-animal-${animal.id}`}
            />
            <div className="absolute top-2 right-2">
              <Badge className={`${rarityColors[animal.rarity]} text-xs`} data-testid={`badge-rarity-${animal.id}`}>
                {animal.rarity}
              </Badge>
            </div>
            {!animal.collected && (
              <div className="absolute inset-0 bg-foreground/80 flex items-center justify-center">
                <div className="text-background text-6xl">?</div>
              </div>
            )}
          </div>
          <div className="p-3">
            <h3 className="font-playful text-lg text-foreground" data-testid={`text-name-${animal.id}`}>
              {animal.collected ? animal.name : 'Unknown'}
            </h3>
            <p className="text-sm text-muted-foreground" data-testid={`text-species-${animal.id}`}>
              {animal.collected ? animal.species : '???'}
            </p>
          </div>
        </CardContent>
      </Card>
    </motion.div>
  );
}